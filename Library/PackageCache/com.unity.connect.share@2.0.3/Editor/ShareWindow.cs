using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Connect.Share.Editor
{
    /// <summary>
    /// An Editor window that allows the user to share a WebGL build of the game to Unity Connect
    /// </summary>
    public class ShareWindow : EditorWindow
    {
        public const string TAB_INTRODUCTION = "Introduction";
        public const string TAB_NOT_LOGGED_IN = "NotLoggedIn";
        public const string TAB_INSTALL_WEBGL = "InstallWebGl";
        public const string TAB_NO_BUILD = "NoBuild";
        public const string TAB_SUCCESS = "Success";
        public const string TAB_ERROR = "Error";
        public const string TAB_UPLOADING = "Uploading";
        public const string TAB_PROCESSING = "Processing";
        public const string TAB_UPLOAD = "Upload";

        /// <summary>
        /// Finds the first open instance of ShareWindow, if any.
        /// </summary>
        /// <returns></returns>
        public static ShareWindow FindInstance() => Resources.FindObjectsOfTypeAll<ShareWindow>().FirstOrDefault();

        public Store<AppState> Store
        {
            get
            {
                if (m_Store == null)
                {
                    m_Store = CreateStore();
                }
                return m_Store;
            }
        }
        Store<AppState> m_Store;

        /// <summary>
        /// Holds all the Fronted setup methods of the available tabs
        /// </summary>
        static Dictionary<string, Action> tabFrontendSetupMethods;

        public string currentTab { get; private set; }

        ShareStep currentShareStep;
        string previousTab;
        string gameTitle = ShareUtils.DefaultGameName;
        bool webGLIsInstalled;
        StyleSheet lastCommonStyleSheet; // Dark/Light theme

        [UserSetting("Publish WebGL Game", "Show first-time instructions")]
        static UserSetting<bool> openedForTheFirstTime = new UserSetting<bool>(ShareSettingsManager.instance, "firstTime", true, SettingsScope.Project);

        [MenuItem("Publish/WebGL Project")]
        public static ShareWindow OpenShareWindow()
        {
            var window = GetWindow<ShareWindow>();
            window.Show();
            return window;
        }

        void OnEnable()
        {
            // TODO Bug in Editor/UnityConnect API: loggedIn returns true but token is expired/empty.
            string token = UnityConnectSession.instance.GetAccessToken();
            if (token.Length == 0)
            {
                Store.Dispatch(new NotLoginAction());
            }

            SetupBackend();
            SetupFrontend();
        }

        void OnDisable()
        {
            TeardownBackend();
        }

        void OnBeforeAssemblyReload()
        {
            SessionState.SetString(typeof(ShareWindow).Name, EditorJsonUtility.ToJson(Store));
        }

        static Store<AppState> CreateStore()
        {
            var shareState = JsonUtility.FromJson<AppState>(SessionState.GetString(typeof(ShareWindow).Name, "{}"));
            return new Store<AppState>(ShareReducer.reducer, shareState, ShareMiddleware.Create());
        }

        void Update()
        {
            if (currentShareStep != Store.state.step)
            {
                string token = UnityConnectSession.instance.GetAccessToken();
                if (token.Length != 0)
                {
                    currentShareStep = Store.state.step;
                    return;
                }
                Store.Dispatch(new NotLoginAction());
            }
            RebuildFrontend();
        }

        void SetupFrontend()
        {
            titleContent.text = "Publish";
            minSize = new Vector2(300f, 300f);
            maxSize = new Vector2(600f, 600f);
            RebuildFrontend();
        }

        void RebuildFrontend()
        {
            if (!string.IsNullOrEmpty(Store.state.errorMsg))
            {
                LoadTab(TAB_ERROR);
                return;
            }

            if (openedForTheFirstTime.value)
            {
                LoadTab(TAB_INTRODUCTION);
                return;
            }

            if (currentShareStep != Store.state.step)
            {
                currentShareStep = Store.state.step;
            }

            bool loggedOut = (currentShareStep == ShareStep.Login);
            if (loggedOut)
            {
                LoadTab(TAB_NOT_LOGGED_IN);
                return;
            }

            if (!webGLIsInstalled)
            {
                UpdateWebGLInstalledFlag();
                LoadTab(TAB_INSTALL_WEBGL);
                return;
            }

            if (!ShareUtils.ValidBuildExists())
            {
                LoadTab(TAB_NO_BUILD);
                return;
            }

            if (!string.IsNullOrEmpty(Store.state.url))
            {
                LoadTab(TAB_SUCCESS);
                return;
            }


            if (currentShareStep == ShareStep.Upload)
            {
                LoadTab(TAB_UPLOADING);
                return;
            }

            if (currentShareStep == ShareStep.Process)
            {
                LoadTab(TAB_PROCESSING);
                return;
            }

            LoadTab(TAB_UPLOAD);
        }

        void SetupBackend()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            currentShareStep = Store.state.step;
            currentTab = string.Empty;
            previousTab = string.Empty;
            UpdateWebGLInstalledFlag();

            tabFrontendSetupMethods = new Dictionary<string, Action>
            {
                { TAB_INTRODUCTION, SetupIntroductionTab },
                { TAB_NOT_LOGGED_IN, SetupNotLoggedInTab },
                { TAB_INSTALL_WEBGL, SetupInstallWebGLTab },
                { TAB_NO_BUILD, SetupNoBuildTab },
                { TAB_SUCCESS, SetupSuccessTab },
                { TAB_ERROR, SetupErrorTab },
                { TAB_UPLOADING, SetupUploadingTab },
                { TAB_PROCESSING, SetupProcessingTab },
                { TAB_UPLOAD, SetupUploadTab }
            };
        }

        void TeardownBackend()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            Store.Dispatch(new DestroyAction());
        }

        void LoadTab(string tabName)
        {
            if (!CanSwitchToTab(tabName)) { return; }
            previousTab = currentTab;
            currentTab = tabName;
            rootVisualElement.Clear();

            string uxmlDefinitionFilePath = string.Format("Packages/com.unity.connect.share/UI/{0}.uxml", tabName);
            VisualTreeAsset windowContent = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlDefinitionFilePath);
            windowContent.CloneTree(rootVisualElement);

            //preserve the base style, remove all styles defined in UXML and apply new skin
            StyleSheet sheet = rootVisualElement.styleSheets[0];
            rootVisualElement.styleSheets.Clear();
            rootVisualElement.styleSheets.Add(sheet);
            UpdateWindowSkin();

            tabFrontendSetupMethods[tabName].Invoke();
        }

        void UpdateWindowSkin()
        {
            RemoveStyleSheet(lastCommonStyleSheet, rootVisualElement);

            string theme = EditorGUIUtility.isProSkin ? "_Dark" : string.Empty;
            string commonStyleSheetFilePath = string.Format("Packages/com.unity.connect.share/UI/Styles{0}.uss", theme);
            lastCommonStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(commonStyleSheetFilePath);
            rootVisualElement.styleSheets.Add(lastCommonStyleSheet);
        }

        bool CanSwitchToTab(string tabName) { return tabName != currentTab; }

        #region Tabs Generation
        void SetupIntroductionTab()
        {
            SetupButton("btnGetStarted", OnGetStartedClicked, true);
        }

        void SetupNotLoggedInTab()
        {
            SetupButton("btnSignIn", OnSignInClicked, true);
        }

        void SetupInstallWebGLTab()
        {
            SetupButton("btnOpenInstallGuide", OnOpenInstallationGuideClicked, true);
        }

        void SetupNoBuildTab()
        {
            SetupButton("btnBuild", OnCreateABuildClicked, true);
            SetupButton("btnLocateExisting", OnLocateBuildClicked, true);
        }

        void SetupSuccessTab()
        {
            AnalyticsHelper.UploadCompleted(UploadResult.Succeeded);
            UpdateHeader();
            SetupLabel("lblLink", "Click here if nothing happens", rootVisualElement, new ShareUtils.LeftClickManipulator(OnProjectLinkClicked));
            SetupButton("btnFinish", OnFinishClicked, true);
            OpenConnectUrl(Store.state.url);
        }

        void SetupErrorTab()
        {
            SetupLabel("lblError", Store.state.errorMsg);
            SetupButton("btnBack", OnBackClicked, true);
        }

        void SetupUploadingTab()
        {
            UpdateHeader();
            SetupButton("btnCancel", OnCancelUploadClicked, true);
        }

        void SetupProcessingTab()
        {
            UpdateHeader();
            SetupButton("btnCancel", OnCancelUploadClicked, true);
        }

        public static VisualTreeAsset LoadUXML(string name) { return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(string.Format("Packages/com.unity.connect.share/UI/{0}.uxml", name)); }

        void SetupUploadTab()
        {
            List<string> existingBuildsPaths = ShareUtils.GetAllBuildsDirectories();
            VisualElement buildsList = rootVisualElement.Query<VisualElement>("buildsList");
            buildsList.contentContainer.Clear();

            VisualTreeAsset containerTemplate = LoadUXML("BuildContainerTemplate");
            VisualElement containerInstance;

            for (int i = 0; i < ShareUtils.MAX_DISPLAYED_BUILDS; i++)
            {
                containerInstance = containerTemplate.CloneTree().Q("buildContainer");
                SetupBuildContainer(containerInstance, existingBuildsPaths[i]);
                buildsList.contentContainer.Add(containerInstance);
            }

            SetupButton("btnNewBuild", OnCreateABuildClicked, true);

            ToolbarMenu helpMenu = rootVisualElement.Q<ToolbarMenu>("menuHelp");
            helpMenu.menu.AppendAction("Open Build Settings...", a => { OnOpenBuildSettingsClicked(); }, a => DropdownMenuAction.Status.Normal);
            helpMenu.menu.AppendAction("Locate Build...", a => { OnLocateBuildClicked(); }, a => DropdownMenuAction.Status.Normal);
            helpMenu.menu.AppendAction("WebGL Build Tutorial", a => { OnOpenHelpClicked(); }, a => DropdownMenuAction.Status.Normal);

            //hide the dropdown arrow
            IEnumerator<VisualElement> helpMenuChildrenEnumerator = helpMenu.Children().GetEnumerator();
            helpMenuChildrenEnumerator.MoveNext(); //get to the label (to ignore)
            helpMenuChildrenEnumerator.MoveNext(); //get to the dropdown arrow (to hide)
            helpMenuChildrenEnumerator.Current.visible = false;
        }

        void UpdateHeader()
        {
            gameTitle = ShareUtils.GetFilteredGameTitle(gameTitle);
            SetupLabel("lblProjectName", gameTitle, rootVisualElement, new ShareUtils.LeftClickManipulator(OnProjectLinkClicked));
            SetupLabel("lblUserEmail", string.Format("By {0}", CloudProjectSettings.userName));
            SetupImage("imgThumbnail", ShareUtils.GetThumbnailPath());
        }

        #endregion

        #region UI Events and Callbacks

        void OnBackClicked()
        {
            Store.Dispatch(new DestroyAction());
            LoadTab(previousTab);
        }

        void OnGetStartedClicked()
        {
            openedForTheFirstTime.SetValue(false);
        }

        void OnSignInClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_SignIn", currentTab));
            UnityConnectSession.instance.ShowLogin();
        }

        void OnOpenInstallationGuideClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_OpenInstallationGuide", currentTab));
            Application.OpenURL("https://learn.unity.com/tutorial/fps-mod-share-your-game-on-the-web?projectId=5d9c91a4edbc2a03209169ab#5db306f5edbc2a001f7a307d");
        }

        void OnOpenHelpClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_OpenHelp", currentTab));
            Application.OpenURL("https://learn.unity.com/tutorial/fps-mod-share-your-game-on-the-web?projectId=5d9c91a4edbc2a03209169ab#5db306f5edbc2a001f7a307d");
        }

        void OnLocateBuildClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_LocateBuild", currentTab));
            string previousBuildPath = ShareUtils.GetFirstValidBuildPath();
            string buildPath = EditorUtility.OpenFolderPanel("Choose folder", string.IsNullOrEmpty(previousBuildPath) ? Application.persistentDataPath : previousBuildPath, string.Empty);
            if (string.IsNullOrEmpty(buildPath)) { return; }
            if (!ShareUtils.BuildIsValid(buildPath))
            {
                Store.Dispatch(new OnErrorAction() { errorMsg = "This build is corrupted or missing, please delete it and choose another one to share" });
                return;
            }
            ShareUtils.AddBuildDirectory(buildPath);
            if (currentTab != TAB_UPLOAD) { return; }
            SetupUploadTab();
        }

        void OnOpenBuildSettingsClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_OpenBuildSettings", currentTab));
            BuildPlayerWindow.ShowBuildPlayerWindow();
        }

        void OnCreateABuildClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_CreateBuild", currentTab));
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                if (!ShowSwitchToWebGLPopup()) { return; } //Debug.LogErrorFormat("Switching from {0} to {1}", EditorUserBuildSettings.activeBuildTarget, BuildTarget.WebGL);
            }
            OnWebGLBuildTargetSet();
        }

        void OnFinishClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_Finish", currentTab));
            Store.Dispatch(new DestroyAction());
        }

        void OnCancelUploadClicked()
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_CancelUpload", currentTab));
            AnalyticsHelper.UploadCompleted(UploadResult.Cancelled);
            Store.Dispatch(new StopUploadAction());
        }

        void OnOpenBuildFolderClicked(string buildPath)
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_OpenBuildFolder", currentTab));
            EditorUtility.RevealInFinder(buildPath);
        }

        void OnShareClicked(string gameBuildPath)
        {
            AnalyticsHelper.ButtonClicked(string.Format("{0}_Publish", currentTab));
            if (!ShareUtils.BuildIsValid(gameBuildPath))
            {
                Store.Dispatch(new OnErrorAction() { errorMsg = "This build is corrupted or missing, please delete it and choose another one to publish" });
                return;
            }

            Store.Dispatch(new ShareStartAction() { title = gameTitle, buildPath = gameBuildPath });
        }

        void OnDeleteClicked(string buildPath, string gameTitle)
        {
            if (!Directory.Exists(buildPath))
            {
                Store.Dispatch(new OnErrorAction() { errorMsg = "Build folder not found" });
                return;
            }

            switch (ShowDeleteBuildPopup(gameTitle))
            {
                case 0: // Yes
                    AnalyticsHelper.ButtonClicked(string.Format("{0}_Delete_RemoveFromList", currentTab));
                    ShareUtils.RemoveBuildDirectory(buildPath);
                    SetupUploadTab();
                    break;

                case 1: break; // Cancel

                case 2: // Yes and delete
                    AnalyticsHelper.ButtonClicked(string.Format("{0}_Delete_RemoveBuildFiles", currentTab));
                    ShareUtils.RemoveBuildDirectory(buildPath);
                    Directory.Delete(buildPath, true);
                    SetupUploadTab();
                    break;
            }
        }

        internal void OnUploadProgress(int percentage)
        {
            if (currentTab != TAB_UPLOADING) { return; }

            ProgressBar progressBar = rootVisualElement.Query<ProgressBar>("barProgress");
            progressBar.value = percentage;
            SetupLabel("lblProgress", string.Format("Uploading ({0}%)...", percentage));
        }

        internal void OnProcessingProgress(int percentage)
        {
            if (currentTab != TAB_PROCESSING) { return; }

            ProgressBar progressBar = rootVisualElement.Query<ProgressBar>("barProgress");
            progressBar.value = percentage;
            SetupLabel("lblProgress", string.Format("Processing ({0}%)...", percentage));
        }

        #endregion

        #region UI Setup Helpers

        void SetupBuildContainer(VisualElement container, string buildPath)
        {
            if (ShareUtils.BuildIsValid(buildPath))
            {
                string gameTitle = buildPath.Split('/').Last();
                SetupButton("btnOpenFolder", () => OnOpenBuildFolderClicked(buildPath), true, container, "Reveal Build Folder");
                SetupButton("btnDelete", () => OnDeleteClicked(buildPath, gameTitle), true, container, "Delete Build");
                SetupButton("btnShare", () => OnShareClicked(buildPath), true, container, "Publish WebGL Build to Unity Connect");
                SetupLabel("lblLastBuildInfo", string.Format("Created: {0} with Unity {1}", File.GetLastWriteTime(buildPath), ShareUtils.GetUnityVersionOfBuild(buildPath)), container);
                SetupLabel("lblGameTitle", gameTitle, container);
                SetupLabel("lblBuildSize", string.Format("Build Size: {0}", ShareUtils.FormatBytes(ShareUtils.GetSizeFolderSize(buildPath))), container);
                container.style.display = DisplayStyle.Flex;
                return;
            }

            SetupButton("btnOpenFolder", null, false, container);
            SetupButton("btnDelete", null, false, container);
            SetupButton("btnShare", null, false, container);
            SetupLabel("lblGameTitle", "-", container);
            SetupLabel("lblLastBuildInfo", "-", container);
            container.style.display = DisplayStyle.None;
        }

        void SetupButton(string buttonName, Action onClickAction, bool isEnabled, VisualElement parent = null, string tooltip = "")
        {
            parent = parent ?? rootVisualElement;
            Button button = parent.Query<Button>(buttonName);
            button.SetEnabled(isEnabled);
            button.clickable = new Clickable(() => onClickAction.Invoke());
            button.tooltip = string.IsNullOrEmpty(tooltip) ? button.text : tooltip;
        }

        void SetupLabel(string labelName, string text, VisualElement parent = null, Manipulator manipulator = null)
        {
            if (parent == null)
            {
                parent = rootVisualElement;
            }
            Label label = parent.Query<Label>(labelName);
            label.text = text;
            if (manipulator == null) { return; }
            label.AddManipulator(manipulator);
        }

        static void OnProjectLinkClicked(VisualElement label)
        {
            OpenConnectUrl(FindInstance().Store.state.url);
        }

        static void OpenConnectUrl(string url)
        {
            if (UnityConnectSession.instance.GetAccessToken().Length > 0)
                UnityConnectSession.OpenAuthorizedURLInWebBrowser(url);
            else
                Application.OpenURL(url);
        }

        void SetupImage(string imageName, string imagePath)
        {
            Texture2D imageToLoad = new Texture2D(2, 2);
            if (!File.Exists(imagePath))
            {
                //[TODO] Load some placeholder image and remove the return statement
                return;
            }
            else
            {
                imageToLoad.LoadImage(File.ReadAllBytes(imagePath));
            }
            Image image = rootVisualElement.Query<Image>(imageName);
            image.image = imageToLoad;
        }

        static bool ShowSwitchToWebGLPopup()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Could not switch platform because Unity is compiling!");
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Could not switch platform because Unity is in Play Mode!");
                return false;
            }

            string title = "Switch Platform";
            string message = "It seems that you have not selected WebGL platform. Would you like to switch now?";
            string yesButtonText = "Switch to WebGL";
            string noButtonText = "Cancel";

            bool yesButtonClicked = EditorUtility.DisplayDialog(title, message, yesButtonText, noButtonText);
            if (yesButtonClicked)
            {
                AnalyticsHelper.ButtonClicked("Popup_SwitchPlatform_Yes");
            }
            else
            {
                AnalyticsHelper.ButtonClicked("Popup_SwitchPlatform_No");
            }
            return yesButtonClicked;
        }

        static int ShowDeleteBuildPopup(string gameTitle)
        {
            string title = "Remove Build";
            string message = string.Format("Do you just want to remove \"{0}\" from the list or also delete all the build files?", gameTitle);
            string yesButtonText = "Remove from List";
            string yesAndDeleteButtonText = "Delete Build Files";
            string noButtonText = "Cancel";

            return EditorUtility.DisplayDialogComplex(title, message, yesButtonText, noButtonText, yesAndDeleteButtonText);
        }

        static void RemoveStyleSheet(StyleSheet styleSheet, VisualElement target)
        {
            if (!styleSheet) { return; }
            if (!target.styleSheets.Contains(styleSheet)) { return; }
            target.styleSheets.Remove(styleSheet);
        }

        #endregion

        public void OnWebGLBuildTargetSet()
        {
            bool buildSettingsHaveNoActiveScenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes).Length == 0;
            if (buildSettingsHaveNoActiveScenes)
            {
                BuildPlayerWindow.ShowBuildPlayerWindow();
                return;
            }

            if (!ShareBuildProcessor.OpenBuildGameDialog(BuildTarget.WebGL)) { return; }
            if (currentTab != TAB_UPLOAD) { return; }
            SetupUploadTab();
        }

        void UpdateWebGLInstalledFlag()
        {
            webGLIsInstalled = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        }
    }
}
