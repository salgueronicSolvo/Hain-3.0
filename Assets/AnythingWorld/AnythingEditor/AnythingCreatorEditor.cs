using AnythingWorld.Networking.Editor;
using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AnythingWorld.Editor
{
    [Serializable]
    public class StateTexture2D
    {
        public Texture2D activeTexture;
        public Texture2D inactiveTexture;
        public Texture2D hoverTexture;

        public bool TexturesLoadedNoHover => activeTexture != null && inactiveTexture != null;
        public bool TexturesLoadedHover => activeTexture != null && inactiveTexture != null && hoverTexture != null;

        public StateTexture2D(Texture2D activeTexture, Texture2D inactiveTexture, Texture2D hoverTexture = null)
        {
            this.activeTexture = activeTexture;
            this.inactiveTexture = inactiveTexture;
            this.hoverTexture = hoverTexture;
        }
    }

    public class AnythingCreatorEditor : AnythingEditor
    {
        #region Fields
        public enum CreationSearchCategory
        {
            MODELS, WORLDS, LIGHTING, COLLECTION
        }
        protected string windowTitle;
        private float searchRingAngle = 0;
        private double lastEditorTime = 0;
        protected static float resultThumbnailMultiplier = 1f;
        protected List<SearchResult> searchResults = null;
        protected List<SearchResult> filteredResults = null;
        protected enum SearchMode { IDLE, RUNNING, RUNNING_SILENTLY, FAILURE, SUCCESS }

        protected SearchMode searchMode = SearchMode.IDLE;
        protected string searchModeFailReason = "";

        protected Color bannerTintA, bannerTintB;

        #region Transform Settings
        protected Transform objectParentTransform;
        protected Vector3 objectPosition;
        protected Vector3 objectRotation;
        protected float objectScaleMultiplier = 1f;
        protected bool transformFieldsEnabled = false;
        protected bool customParentTransformEnabled = false;
        protected bool customPositionFieldEnabled = false;
        protected bool customRotationFieldEnabled = false;
        protected bool customScaleMultiplierEnabled = false;
        protected bool makeStatic = false;
        protected bool showGridHandles = false;
        protected bool addDefaultBehaviour = false;
        private bool defaultVehicleBehaviourEnabled = true;
        private bool defaultFlyingVehicleBehaviourEnabled = true;
        private bool defaultRiggedBehaviourEnabled = true;
        private bool defaultStaticBehaviourEnabled = true;
        private bool defaultShaderBehaviourEnabled = true;
        protected MonoScript defaultRiggedScript;
        protected MonoScript defaultVehicleScript;
        protected MonoScript defaultFlyingScript;
        protected MonoScript defaultStaticScript;
        private MonoScript defaultShaderScript;



        protected Dictionary<AnimationPipeline, Type> DefaultBehavioursArray
        {
            get
            {
                var temp = new Dictionary<AnimationPipeline, Type>();
                if (defaultRiggedBehaviourEnabled && defaultRiggedScript != null) temp.Add(AnimationPipeline.Rigged, defaultRiggedScript?.GetClass());
                if (defaultVehicleBehaviourEnabled && defaultVehicleScript != null) temp.Add(AnimationPipeline.WheeledVehicle, defaultVehicleScript?.GetClass());
                if (defaultFlyingVehicleBehaviourEnabled && defaultFlyingScript != null) temp.Add(AnimationPipeline.PropellorVehicle, defaultFlyingScript?.GetClass());
                if (defaultStaticBehaviourEnabled && defaultStaticScript != null) temp.Add(AnimationPipeline.Static, defaultStaticScript.GetClass());
                return temp;
            }
        }

        protected bool addCustomBehaviours = false;
        protected Dictionary<string, UnityEngine.Object> customBehaviourDictionary = new Dictionary<string, UnityEngine.Object>();

        protected bool showGridOptionsDrawer = false;
        protected bool showDefaultBehavioursDrawer = false;
        protected bool showTransformDrawer = false;
        protected bool showGeneralDrawer = false;
        protected bool gridPlacementEnabled = false;
        protected bool transformSettingsActive = false;
        #endregion Transform Settings
        #region Filters
        protected DropdownOption[] CategoryFilter
        {
            get
            {
                string[] categoryLabels = {
                    "All", "Animals & Pets", "Architecture", "Art & Abstract",
            "Cars & Vehicles", "Characters & Creatures", "Cultural Heritage & History",
            "Electronics & Gadgets","Fashion & Style","Food & Drink","Furniture & Home",
            "Music","Nature & Plants","News & Politics","People","Places & Travel",
            "Science & Technology","Sports & Fitness","Weapons & Military"};
                if (categoryFilter == null)
                {
                    var dropdownList = new List<DropdownOption>();
                    foreach (var (label, index) in categoryLabels.WithIndex())
                    {
                        var option = new DropdownOption()
                        {
                            dataEndpoint = (CategoryDropdownOption)index,
                            label = categoryLabels[index],
                            function = () =>
                            {
                                currentCategory = (CategoryDropdownOption)index;
                                FilterSearchResult(searchResults);
                            }
                        };

                        dropdownList.Add(option);
                    }
                    categoryFilter = dropdownList.ToArray();
                }
                return categoryFilter;
            }
        }
        protected DropdownOption[] categoryFilter;

        protected DropdownOption[] AnimationFilter
        {
            get
            {
                if (animationFilter == null)
                {
                    string[] animationLabels = { "Animated & Still", "Animated Only", "Still Only" };
                    var dropdownList = new List<DropdownOption>();
                    foreach (var (label, index) in animationLabels.WithIndex())
                    {
                        var option = new DropdownOption()
                        {
                            dataEndpoint = (AnimatedDropdownOption)index,
                            label = animationLabels[index],
                            function = () =>
                            {
                                currentAnimationFilter = (AnimatedDropdownOption)index;
                                FilterSearchResult(searchResults);
                            }
                        };

                        dropdownList.Add(option);
                    }
                    animationFilter = dropdownList.ToArray();
                }
                return animationFilter;
            }
        }
        protected DropdownOption[] animationFilter;

        protected DropdownOption[] SortingFilter
        {
            get
            {
                if (sortingFilter == null)
                {
                    string[] sortingLabels = { "Most Relevant",/*"Most Used",*/ "Most Liked", "Liked Models", "A-Z", "Z-A" };
                    var dropdownList = new List<DropdownOption>();
                    foreach (var (label, index) in sortingLabels.WithIndex())
                    {
                        var option = new DropdownOption()
                        {
                            dataEndpoint = (SortingDropdownOption)index,
                            label = sortingLabels[index],
                            function = () =>
                            {
                                currentSortingMethod = (SortingDropdownOption)index;
                                FilterSearchResult(searchResults);
                            }
                        };

                        dropdownList.Add(option);
                    }
                    sortingFilter = dropdownList.ToArray();
                }
                return sortingFilter;
            }
        }
        protected DropdownOption[] sortingFilter;


        protected enum CategoryDropdownOption { ALL, ANIMAL, ARCHITECTURE, ART, CAR, CHARACTER, CULTURE, ELECTRONICS, FASHION, FOOD, FURNITURE, MUSIC, NATURE, NEWS, PEOPLE, PLACE, SCIENCE, SPORTS, WEAPONS }
        protected CategoryDropdownOption currentCategory = CategoryDropdownOption.ALL;

        protected enum AnimatedDropdownOption { BOTH, ANIMATED, STILL }
        protected AnimatedDropdownOption currentAnimationFilter = AnimatedDropdownOption.BOTH;

        protected SortingDropdownOption currentSortingMethod = SortingDropdownOption.MostRelevant;
        #endregion Filters
        #region Styles
        protected GUIStyle iconStyle;


        protected GUIStyle TabButtonInactiveStyle;
        protected GUIStyle TabButtonActiveStyle;
        protected GUIStyle ButtonActiveStyle;
        protected GUIStyle ButtonInactiveStyle;

        protected GUIStyle ModelNameStyle;
        protected GUIStyle AuthorNameStyle;
        protected GUIStyle VoteStyle;
        #endregion Styles
        #region Textures
        #region Base Textures
        private static Texture2D baseWebsiteIcon;
        protected static Texture2D BaseWebsiteIcon
        {
            get
            {
                if (baseWebsiteIcon == null)
                {
                    baseWebsiteIcon = Resources.Load("Editor/Shared/SocialIcons/website") as Texture2D;
                }
                return baseWebsiteIcon;
            }
        }
        private static Texture2D baseDiscordIcon;
        protected static Texture2D BaseDiscordIcon
        {
            get
            {
                if (baseDiscordIcon == null)
                {
                    baseDiscordIcon = Resources.Load("Editor/Shared/SocialIcons/discord") as Texture2D;
                }
                return baseDiscordIcon;
            }
        }
        private static Texture2D baseLoginIcon;
        protected static Texture2D BaseLoginIcon
        {
            get
            {
                if (baseLoginIcon == null)
                {
                    baseLoginIcon = Resources.Load("Editor/Shared/SocialIcons/login") as Texture2D;
                }
                return baseLoginIcon;
            }
        }
        private static Texture2D baseLogoutIcon;
        protected static Texture2D BaseLogoutIcon
        {
            get
            {
                if (baseLogoutIcon == null)
                {
                    baseLogoutIcon = Resources.Load("Editor/Shared/SocialIcons/logout") as Texture2D;
                }
                return baseLogoutIcon;
            }
        }

        private static Texture2D baseTabFrame;
        protected static Texture2D BaseTabFrame
        {
            get
            {
                if (baseTabFrame == null)
                {
                    baseTabFrame = Resources.Load("Editor/AnythingBrowser/buttonFrame") as Texture2D;
                }
                return baseTabFrame;
            }
        }

        private static Texture2D baseLoadingCircle;
        protected static Texture2D BaseLoadingCircle
        {
            get
            {
                if (baseLoadingCircle == null)
                {
                    baseLoadingCircle = Resources.Load("Editor/AnythingBrowser/loadingCircle") as Texture2D;
                }
                return baseLoadingCircle;
            }
        }

        private static Texture2D baseResetIcon;
        protected static Texture2D BaseResetIcon
        {
            get
            {
                if (baseResetIcon == null)
                {
                    baseResetIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/reset") as Texture2D;
                }
                return baseResetIcon;
            }
        }
        private static Texture2D baseTransformIcon;
        protected static Texture2D BaseGridIcon
        {
            get
            {
                if (baseTransformIcon == null)
                {
                    baseTransformIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/transform") as Texture2D;
                }
                return baseTransformIcon;
            }
        }
        private static Texture2D baseGridIcon;
        protected static Texture2D BaseTransformIcon
        {
            get
            {
                if (baseGridIcon == null)
                {
                    baseGridIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/grid") as Texture2D;
                }
                return baseGridIcon;
            }
        }
        private static Texture2D baseClearIcon;
        protected static Texture2D BaseClearIcon
        {
            get
            {
                if (baseClearIcon == null)
                {
                    baseClearIcon = Resources.Load("Editor/AnythingBrowser/Icons/clear") as Texture2D;
                }
                return baseClearIcon;
            }
        }

        private static Texture2D baseInputFieldLeft;
        protected static Texture2D BaseInputFieldLeft
        {
            get
            {
                if (baseInputFieldLeft == null)
                {
                    baseInputFieldLeft = Resources.Load("Editor/AnythingBrowser/roundedEdgeLeft") as Texture2D;
                }
                return baseInputFieldLeft;
            }
        }
        private static Texture2D baseInputFieldRight;
        protected static Texture2D BaseInputFieldRight
        {
            get
            {
                if (baseInputFieldRight == null)
                {
                    baseInputFieldRight = Resources.Load("Editor/AnythingBrowser/roundedEdgeRight") as Texture2D;
                }
                return baseInputFieldRight;
            }
        }
        private static Texture2D baseInputFieldMain;
        protected static Texture2D BaseInputFieldMain
        {
            get
            {
                if (baseInputFieldMain == null)
                {
                    baseInputFieldMain = Resources.Load("Editor/AnythingBrowser/roundedFrameMiddle") as Texture2D;
                }
                return baseInputFieldMain;
            }
        }

        private static Texture2D baseThumbnailGridIcon;
        protected static Texture2D BaseThumbnailGridIcon
        {
            get
            {
                if (baseThumbnailGridIcon == null)
                {
                    baseThumbnailGridIcon = Resources.Load("Editor/AnythingBrowser/Icons/thumbnailGrid") as Texture2D;
                }

                return baseThumbnailGridIcon;
            }
        }
        #region Card
        private static Texture2D baseCardBackdrop;
        protected static Texture2D BaseCardBackdrop
        {
            get
            {
                if (baseCardBackdrop == null)
                {
                    baseCardBackdrop = Resources.Load("Editor/AnythingBrowser/Cards/cardBackdrop") as Texture2D;
                }
                return baseCardBackdrop;
            }
        }
        private static Texture2D[] baseCardThumbnailBackdrops;
        protected static Texture2D[] BaseCardThumbnailBackdrops
        {
            get
            {

                if (baseCardThumbnailBackdrops == null)
                {
                    baseCardThumbnailBackdrops = new[]
                    {
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop1") as Texture2D,
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop2") as Texture2D,
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop3") as Texture2D,
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop4") as Texture2D
                    };
                }
                return baseCardThumbnailBackdrops;
            }
        }
        private static Texture2D baseUserIcon;
        protected static Texture2D BaseUserIcon
        {
            get
            {
                if (baseUserIcon == null)
                {
                    baseUserIcon = Resources.Load("Editor/AnythingBrowser/Cards/cardProfile") as Texture2D;
                }
                return baseUserIcon;
            }
        }

        private static Texture2D baseFilledHeart;
        protected static Texture2D BaseFilledHeart
        {
            get
            {
                if (baseFilledHeart == null)
                {
                    baseFilledHeart = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/filledHeart") as Texture2D;
                }
                return baseFilledHeart;
            }
        }
        private static Texture2D baseEmptyHeart;
        protected static Texture2D BaseEmptyHeart
        {
            get
            {
                if (baseEmptyHeart == null)
                {
                    baseEmptyHeart = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/emptyHeart") as Texture2D;
                }
                return baseEmptyHeart;
            }
        }

        private static Texture2D baseCollectionIcon;
        protected static Texture2D BaseCollectionIcon
        {
            get
            {
                if (baseCollectionIcon == null)
                {
                    baseCollectionIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/addToList") as Texture2D;
                }
                return baseCollectionIcon;
            }
        }
        private static Texture2D baseAnimatedIcon;
        protected static Texture2D BaseAnimatedIcon
        {
            get
            {
                if (baseAnimatedIcon == null)
                {
                    baseAnimatedIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/animated") as Texture2D;
                }
                return baseAnimatedIcon;
            }
        }

        private static Texture2D baseCardObjectIcon;
        protected static Texture2D BaseCardObjectIcon
        {
            get
            {
                if (baseCardObjectIcon == null)
                {
                    baseCardObjectIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/object") as Texture2D;
                }
                return baseCardObjectIcon;
            }
        }
        private static Texture2D baseCardWorldIcon;
        protected static Texture2D BaseCardWorldIcon
        {
            get
            {
                if (baseCardWorldIcon == null)
                {
                    baseCardWorldIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/world") as Texture2D;
                }
                return baseCardWorldIcon;
            }
        }
        private static Texture2D baseCardLightingIcon;
        protected static Texture2D BaseCardLightingIcon
        {
            get
            {
                if (baseCardLightingIcon == null)
                {
                    baseCardLightingIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/lighting") as Texture2D;
                }
                return baseCardLightingIcon;
            }
        }
        private static Texture2D baseCardCollectionIcon;
        protected static Texture2D BaseCardCollectionIcon
        {
            get
            {
                if (baseCardCollectionIcon == null)
                {
                    baseCardCollectionIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/collections") as Texture2D;
                }
                return baseCardCollectionIcon;
            }
        }

        private static Texture2D baseButtonTint;
        protected static Texture2D BaseButtonTint
        {
            get
            {
                if (baseButtonTint == null)
                {
                    baseButtonTint = Resources.Load("Editor/AnythingBrowser/thumbnailBackgroundActive") as Texture2D;
                }
                return baseButtonTint;
            }
        }
        #endregion Card
        #endregion Base Textures
        #region Tinted Textures
        private Texture2D tintedUserIcon;
        protected Texture2D TintedUserIcon
        {
            get
            {
                if (tintedUserIcon == null)
                {
                    tintedUserIcon = TintTextureToEditorTheme(BaseUserIcon);
                }
                return tintedUserIcon;
            }
            set => tintedUserIcon = value;
        }

        private Texture2D blackAnythingGlobeLogo;
        protected Texture2D BlackAnythingGlobeLogo
        {
            get
            {
                if (blackAnythingGlobeLogo == null)
                {
                    blackAnythingGlobeLogo = TintTexture(BaseAnythingGlobeLogo, Color.black);
                }
                return blackAnythingGlobeLogo;
            }
            set => blackAnythingGlobeLogo = value;
        }

        private Texture2D tintedGradientBanner;
        protected Texture2D TintedGradientBanner
        {
            get
            {
                if (tintedGradientBanner == null)
                {
                    tintedGradientBanner = TintGradient(BaseGradientBanner, bannerTintA, bannerTintB);
                }
                return tintedGradientBanner;
            }
            set => tintedGradientBanner = value;
        }

        private Texture2D tintedLoadingCircle;
        protected Texture2D TintedLoadingCircle
        {
            get
            {
                if (tintedLoadingCircle == null)
                {
                    tintedLoadingCircle = TintTextureToEditorTheme(BaseLoadingCircle, Color.white, Color.black);
                }
                return tintedLoadingCircle;
            }
            set => tintedLoadingCircle = value;
        }

        private Texture2D tintedInputFieldMain;
        protected Texture2D TintedInputFieldMain
        {
            get
            {
                if (tintedInputFieldMain == null)
                {
                    tintedInputFieldMain = TintTextureToEditorTheme(BaseInputFieldMain, HexToColour("292A2B"), HexToColour("E4E5E3"));
                }

                return tintedInputFieldMain;
            }
            set => tintedInputFieldMain = value;
        }
        private Texture2D tintedInputFieldLeft;
        protected Texture2D TintedInputFieldLeft
        {
            get
            {
                if (tintedInputFieldLeft == null)
                {
                    tintedInputFieldLeft = TintTextureToEditorTheme(BaseInputFieldLeft, HexToColour("292A2B"), HexToColour("E4E5E3"));
                }

                return tintedInputFieldLeft;
            }
            set => tintedInputFieldLeft = value;
        }
        private Texture2D tintedInputFieldRight;
        protected Texture2D TintedInputFieldRight
        {
            get
            {
                if (tintedInputFieldRight == null)
                {
                    tintedInputFieldRight = TintTextureToEditorTheme(BaseInputFieldRight, HexToColour("292A2B"), HexToColour("E4E5E3"));
                }

                return tintedInputFieldRight;
            }
            set => tintedInputFieldRight = value;
        }
        private Texture2D tintedThumbnailGridIcon;
        protected Texture2D TintedThumbnailGridIcon
        {
            get
            {
                if (tintedThumbnailGridIcon == null)
                {
                    tintedThumbnailGridIcon = TintTextureToEditorTheme(BaseThumbnailGridIcon, Color.white, Color.black);
                }
                return tintedThumbnailGridIcon;
            }
            set => tintedThumbnailGridIcon = value;
        }
        #endregion Tinted Textures
        #region State Textures
        private StateTexture2D stateLogoutIcon;
        protected StateTexture2D StateLogoutIcon
        {
            get
            {
                if (stateLogoutIcon == null || !stateLogoutIcon.TexturesLoadedHover)
                {
                    stateLogoutIcon = new StateTexture2D(BaseLogoutIcon, BaseLogoutIcon, TintTexture(BaseLogoutIcon, HexToColour("EEEEEE")));
                }
                return stateLogoutIcon;
            }
            set => stateLogoutIcon = value;
        }
        private StateTexture2D stateLoginIcon;
        protected StateTexture2D StateLoginIcon
        {
            get
            {
                if (stateLoginIcon == null || !stateLoginIcon.TexturesLoadedHover)
                {
                    stateLoginIcon = new StateTexture2D(BaseLoginIcon, BaseLoginIcon, TintTexture(BaseLoginIcon, HexToColour("EEEEEE")));
                }
                return stateLoginIcon;
            }
            set => stateLoginIcon = value;
        }
        private StateTexture2D stateDiscordIcon;
        protected StateTexture2D StateDiscordIcon
        {
            get
            {
                if (stateDiscordIcon == null || !stateDiscordIcon.TexturesLoadedHover)
                {
                    stateDiscordIcon = new StateTexture2D(BaseDiscordIcon, BaseDiscordIcon, TintTexture(BaseDiscordIcon, HexToColour("EEEEEE")));
                }
                return stateDiscordIcon;
            }
            set => stateDiscordIcon = value;
        }
        private StateTexture2D stateWebsiteIcon;
        protected StateTexture2D StateWebsiteIcon
        {
            get
            {
                if (stateWebsiteIcon == null || !stateWebsiteIcon.TexturesLoadedHover)
                {
                    stateWebsiteIcon = new StateTexture2D(BaseWebsiteIcon, BaseWebsiteIcon, TintTexture(BaseWebsiteIcon, HexToColour("EEEEEE")));
                }
                return stateWebsiteIcon;
            }
            set => stateWebsiteIcon = value;
        }

        private StateTexture2D stateTabFrame;
        protected StateTexture2D StateTabFrame
        {
            get
            {
                if (stateTabFrame == null || !stateTabFrame.TexturesLoadedHover)
                {
                    stateTabFrame = new StateTexture2D(
                        TintTextureToEditorTheme(BaseTabFrame, HexToColour("98999A"), HexToColour("979797")),
                        TintTextureToEditorTheme(BaseTabFrame, HexToColour("575859"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseTabFrame, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateTabFrame;
            }
            set => stateTabFrame = value;
        }

        private StateTexture2D stateResetIcon;
        protected StateTexture2D StateResetIcon
        {
            get
            {
                if (stateResetIcon == null || !stateResetIcon.TexturesLoadedHover)
                {
                    stateResetIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseResetIcon, Color.white, Color.black),
                        TintTexture(BaseResetIcon, HexToColour("979797")),
                        TintTextureToEditorTheme(BaseResetIcon, HexToColour("606162"), HexToColour("EDEEEC")));

                }
                return stateResetIcon;
            }
            set => stateResetIcon = value;
        }
        private StateTexture2D stateTransformIcon;
        protected StateTexture2D StateTransformIcon
        {
            get
            {
                if (stateTransformIcon == null || !stateTransformIcon.TexturesLoadedNoHover)
                {
                    stateTransformIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseTransformIcon, Color.black, Color.white),
                        TintTextureToEditorTheme(BaseTransformIcon, Color.white, Color.black));
                }
                return stateTransformIcon;
            }
            set => stateTransformIcon = value;
        }
        private StateTexture2D stateGridIcon;
        protected StateTexture2D StateGridIcon
        {
            get
            {
                if (stateGridIcon == null || !stateGridIcon.TexturesLoadedHover)
                {
                    stateGridIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseGridIcon, Color.white, Color.black),
                        TintTexture(BaseGridIcon, HexToColour("979797")),
                        TintTextureToEditorTheme(BaseGridIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateGridIcon;
            }
            set => stateGridIcon = value;
        }
        private StateTexture2D stateClearIcon;
        protected StateTexture2D StateClearIcon
        {
            get
            {
                if (stateClearIcon == null || !stateClearIcon.TexturesLoadedHover)
                {
                    stateClearIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseClearIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseClearIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseClearIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateClearIcon;
            }
            set => stateClearIcon = value;
        }

        private StateTexture2D stateHeartIcon;
        protected StateTexture2D StateHeartIcon
        {
            get
            {
                if (stateHeartIcon == null || !stateHeartIcon.TexturesLoadedHover)
                {
                    stateHeartIcon = new StateTexture2D(
                        EditorGUIUtility.isProSkin ? BaseFilledHeart : TintTextureWhite(BaseFilledHeart, Color.black),
                        TintTextureToEditorTheme(BaseEmptyHeart, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseEmptyHeart, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateHeartIcon;
            }
            set => stateHeartIcon = value;
        }
        private StateTexture2D stateCollectionIcon;
        protected StateTexture2D StateCollectionIcon
        {
            get
            {
                if (stateCollectionIcon == null || !stateCollectionIcon.TexturesLoadedHover)
                {
                    stateCollectionIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseCollectionIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseCollectionIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseCollectionIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateCollectionIcon;
            }
            set => stateCollectionIcon = value;
        }
        private StateTexture2D stateCardBackdrop;
        protected StateTexture2D StateCardBackdrop
        {
            get
            {
                if (stateCardBackdrop == null || !stateCardBackdrop.TexturesLoadedHover)
                {
                    stateCardBackdrop = new StateTexture2D(
                        TintTextureToEditorTheme(BaseCardBackdrop, HexToColour("3F4041"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseCardBackdrop, HexToColour("3F4041"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseCardBackdrop, HexToColour("575859"), HexToColour("EDEEEC")));
                }
                return stateCardBackdrop;
            }
            set => stateCardBackdrop = value;
        }
        #endregion State Textures
        #endregion Textures

        protected Vector2 newScrollPosition;
        protected bool copiedToKeyboard = false;
        protected Rect copiedRect;
        protected int copiedResult = 0;
        #endregion

        #region Initialization
        protected new void Awake()
        {
            base.Awake();
            AssignDefaultBehavioursFromScriptable();
        }
        protected void AssignDefaultBehavioursFromScriptable()
        {
            if (ScriptableObjectExtensions.TryGetInstance<DefaultBehaviourScriptable>(out var script))
            {
                defaultRiggedScript = script.defaultRigged;
                defaultVehicleScript = script.defaultWheeledVehicle;
                defaultFlyingScript = script.defaultPropellorVehicle;
            }
        }
        protected override void DefineCustomStyles()
        {
            base.DefineCustomStyles();
            iconStyle = new GUIStyle
            {
                normal =
                {
                    background = null
                },
                hover =
                {
                    background = null
                },
                stretchWidth = true,
                clipping = TextClipping.Overflow
            };

            TabButtonInactiveStyle = new GUIStyle(ButtonStyle)
            {
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black,
                    background = StateTabFrame.inactiveTexture
                },
                hover = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black,
                    background = StateTabFrame.hoverTexture
                },
                fixedHeight = 40
            };

            TabButtonActiveStyle = new GUIStyle(ButtonStyle)
            {
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.black : Color.white,
                    background = StateTabFrame.activeTexture
                },
                hover = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.black : Color.white,
                    background = StateTabFrame.activeTexture
                },
                fixedHeight = 40
            };

            ButtonActiveStyle = new GUIStyle(TabButtonActiveStyle)
            {
                fixedHeight = 20,
                fontSize = 10,
                margin = UniformRectOffset(10),
                padding = UniformRectOffset(2)
            };

            ButtonInactiveStyle = new GUIStyle(TabButtonInactiveStyle)
            {
                fixedHeight = 20,
                fontSize = 10,
                margin = UniformRectOffset(10),
                padding = UniformRectOffset(2)
            };

            ModelNameStyle = new GUIStyle(BodyLabelStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Medium),
                clipping = TextClipping.Clip,
                wordWrap = false
            };

            AuthorNameStyle = new GUIStyle(BodyLabelStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Regular),
                clipping = TextClipping.Clip,
                wordWrap = false
            };

            VoteStyle = new GUIStyle(BodyLabelStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Regular)
            };
        }
        #endregion Initialization

        protected new void OnGUI()
        {
            base.OnGUI();

            if (!editorInitialized)
            {
                if (!InitializeResources())
                {
                    return;
                }
            }
            DrawWindowBanner();
        }

        #region Editor Drawing
        /// <summary>
        /// Draws Anything World logo and social buttons for the Anything Creator windows.
        /// </summary>
        private void DrawWindowBanner()
        {

            var globeRect = new Rect(10, 10, 64, 64);
            var bannerRect = new Rect(0, 0, position.width, globeRect.yMax + 10);
            GUI.DrawTexture(bannerRect, TintedGradientBanner);
            GUI.DrawTexture(globeRect, BlackAnythingGlobeLogo);
            var textHeight = 50;
            var textPadding = 15;
            var titleRect = new Rect(globeRect.xMax + 10, bannerRect.height - textHeight - textPadding, position.width - globeRect.xMax, textHeight);
            var anythingWorld = new GUIContent("ANYTHING WORLD");

            GUI.Label(titleRect, anythingWorld, new GUIStyle(EditorStyles.label) { font = GetPoppinsFont(PoppinsStyle.Bold), fontSize = 22, alignment = TextAnchor.UpperLeft, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(titleRect, new GUIContent(windowTitle), new GUIStyle(EditorStyles.label) { font = GetPoppinsFont(PoppinsStyle.Bold), fontSize = 22, alignment = TextAnchor.LowerLeft, normal = new GUIStyleState() { textColor = Color.white } });

            var iconSize = 16f;
            var iconPadding = 4f;
            var iconMargin = (bannerRect.height - ((iconSize * 3) + (iconPadding * 2))) / 2;

            var iconsXPos = bannerRect.xMax - iconSize - iconMargin;
            var iconsYPos = bannerRect.yMin + iconMargin;

            var versionRect = new Rect(bannerRect.center.x, iconsYPos, (bannerRect.width / 2) - iconSize - iconMargin - iconPadding, iconSize);
            var version = new GUIContent(AnythingSettings.PackageVersion);
            GUI.Label(versionRect, version, new GUIStyle(EditorStyles.label) { font = GetPoppinsFont(PoppinsStyle.Medium), fontSize = 12, alignment = TextAnchor.MiddleRight, normal = new GUIStyleState() { textColor = Color.white } });

            var logoutIconRect = new Rect(iconsXPos, iconsYPos, iconSize, iconSize);
            var discordIconRect = new Rect(iconsXPos, logoutIconRect.yMax + iconPadding, iconSize, iconSize);
            var websiteIconRect = new Rect(iconsXPos, discordIconRect.yMax + iconPadding, iconSize, iconSize);

            if (AnythingSettings.Instance.HasAPIKey())
            {
                if (GUI.Button(logoutIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateLogoutIcon.activeTexture), hover = SetStyleState(StateLogoutIcon.hoverTexture) }))
                {
                    var settingsSerializedObject = new SerializedObject(AnythingSettings.Instance);
                    settingsSerializedObject.FindProperty("apiKey").stringValue = "";
                    settingsSerializedObject.FindProperty("email").stringValue = "";
                    settingsSerializedObject.ApplyModifiedProperties();

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Undo.RecordObject(AnythingSettings.Instance, "Logged out");
                    EditorUtility.SetDirty(AnythingSettings.Instance);

                    while (HasOpenInstances<AnythingCreatorEditor>())
                    {
                        var window = GetWindow(typeof(AnythingCreatorEditor));
                        window.Close();
                    }
                }
            }
            if (GUI.Button(discordIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateDiscordIcon.activeTexture), hover = SetStyleState(StateDiscordIcon.hoverTexture) })) System.Diagnostics.Process.Start("https://discord.gg/anythingworld");
            if (GUI.Button(websiteIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateWebsiteIcon.activeTexture), hover = SetStyleState(StateWebsiteIcon.hoverTexture) })) System.Diagnostics.Process.Start("https://www.anything.world/");
            //Mask banner in layouting
            GUILayoutUtility.GetRect(position.width, bannerRect.yMax, GUILayout.MinWidth(500));
        }

        protected void DrawLoading(Rect miscRect)
        {
            var thisTime = EditorApplication.timeSinceStartup;
            var workArea = GUILayoutUtility.GetRect(position.width, position.height - (miscRect.y + miscRect.height));
            var logoSize = workArea.height / 4;
            var spinningRect = new Rect((workArea.width / 2) - (logoSize / 2), workArea.y + (workArea.height / 2) - (logoSize / 2), logoSize, logoSize);
            var logoRect = new Rect(spinningRect.x + (spinningRect.width / 6), spinningRect.y + (spinningRect.height / 6), spinningRect.width * (2f / 3f), spinningRect.height * (2f / 3f));
            var dt = EditorApplication.timeSinceStartup - lastEditorTime;
            var matrixBack = GUI.matrix;
            searchRingAngle += 75f * (float)dt;
            GUIUtility.RotateAroundPivot(searchRingAngle, spinningRect.center);
            GUI.DrawTexture(spinningRect, TintedLoadingCircle);
            GUI.matrix = matrixBack;
            GUI.DrawTexture(logoRect, EditorGUIUtility.isProSkin ? BaseAnythingGlobeLogo : BlackAnythingGlobeLogo);
            lastEditorTime = thisTime;
        }
        protected void DrawError(string searchTerm)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Error searching for that term, please try again.", new GUIStyle(HeaderLabelStyle) { wordWrap = true });
            GUILayout.FlexibleSpace();
        }
        protected void DrawError()
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(searchModeFailReason, new GUIStyle(HeaderLabelStyle) { wordWrap = true });

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset Creator", TabButtonInactiveStyle))
            {
                ResetAnythingWorld(ResetMode.Creator);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        protected void DrawSettingsIcons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" Make Options", transformSettingsActive ? StateTransformIcon.activeTexture : StateTransformIcon.inactiveTexture), transformSettingsActive ? ButtonActiveStyle : ButtonInactiveStyle, GUILayout.MaxWidth((position.width / 3) - DropdownStyle.margin.horizontal - 8)))
            {
                transformSettingsActive = !transformSettingsActive;
            }

            GUILayout.FlexibleSpace();

            var iconSize = 16;
            var settingsIconsRect = GUILayoutUtility.GetRect((position.width / 3) - DropdownStyle.margin.horizontal - 8, iconSize * 2);
            GUILayout.EndHorizontal();

            var marginY = (settingsIconsRect.height - iconSize) / 2;
            var paddingX = iconSize * (2f / 3f);

            var resetIconRect = new Rect(settingsIconsRect.xMax - iconSize - paddingX, settingsIconsRect.y + marginY, iconSize, iconSize);
            var transformIconRect = new Rect(resetIconRect.x - iconSize - paddingX, settingsIconsRect.y + marginY, iconSize, iconSize);

            var labelRect = resetIconRect;
            labelRect.x -= 90;
            labelRect.width = 60;
            if (gridPlacementEnabled)
            {
                GUI.Label(labelRect, "GRID: ON", new GUIStyle(BodyLabelStyle) { fontSize = 12 });
            }
            else
            {
                GUI.Label(labelRect, "GRID: OFF", new GUIStyle(BodyLabelStyle) { fontSize = 12 });
            }


            if (GUI.Button(transformIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(gridPlacementEnabled ? StateGridIcon.inactiveTexture : StateGridIcon.activeTexture), hover = SetStyleState(StateGridIcon.hoverTexture) }))
            {
                gridPlacementEnabled = !gridPlacementEnabled;
            }


            //Dropdown to Reset Grid, Reset Creator, and Reset All
            if (GUI.Button(resetIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateResetIcon.activeTexture), hover = SetStyleState(StateResetIcon.hoverTexture) }))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Reset Scene"), false, () => SetupResetEditorWindow(ResetMode.Scene));
                menu.AddItem(new GUIContent("Reset Creator"), false, () => SetupResetEditorWindow(ResetMode.Creator));
                menu.AddItem(new GUIContent("Reset All"), false, () => SetupResetEditorWindow(ResetMode.All));
                menu.DropDown(resetIconRect);
            }
        }
        protected void DrawTransformSettings()
        {
            #region General Settings Drawer
            DrawerButton(ref showGeneralDrawer, "General");
            if (showGeneralDrawer)
            {
                //DrawerTitle("General");
                //EditorGUILayout.HelpBox("Override animations", MessageType.Info);
                FieldSplitter();
                GUILayout.BeginHorizontal();
                makeStatic = EditorGUILayout.Toggle(makeStatic, GUILayout.Width(20));
                EditorGUILayout.LabelField("Don't Animate Model", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                AnythingSettings.DebugEnabled = EditorGUILayout.Toggle(AnythingSettings.DebugEnabled, GUILayout.Width(20));
                EditorGUILayout.LabelField("Enable Maker Debug Messages", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

            }
            #endregion

            #region Transform Options Drawer
            DrawerButton(ref showTransformDrawer, "Transform Options");
            if (showTransformDrawer)
            {
                EditorGUILayout.HelpBox("Change positioning, rotation and scale of model within the scene.", MessageType.Info);
                //DrawerTitle("TRANSFORM");


                FieldSplitter();
                CustomObjectField(ref objectParentTransform, "Parent Transform", ref customParentTransformEnabled);
                FieldSplitter();
                CustomObjectField(ref objectPosition, "Position", ref customPositionFieldEnabled);
                FieldSplitter();
                CustomObjectField(ref objectRotation, "Rotation", ref customRotationFieldEnabled);
                FieldSplitter();
                CustomObjectField(ref objectScaleMultiplier, "Scale Multiplier", ref customScaleMultiplierEnabled);
                GUI.enabled = true;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All"))
                {
                    customParentTransformEnabled = true;
                    customPositionFieldEnabled = true;
                    customRotationFieldEnabled = true;
                    customScaleMultiplierEnabled = true;

                }
                if (GUILayout.Button("Deselect All"))
                {
                    customParentTransformEnabled = false;
                    customPositionFieldEnabled = false;
                    customRotationFieldEnabled = false;
                    customScaleMultiplierEnabled = false;
                }
                GUILayout.EndHorizontal();
            }
            #endregion

            #region Default Animations Drawer
            DrawerButton(ref showDefaultBehavioursDrawer, "Default Animation Behaviours");
            if (showDefaultBehavioursDrawer)
            {
                //DrawerTitle("DEFAULT");
                EditorGUILayout.HelpBox("Specify the default behaviours added to models for each type of model.", MessageType.Info);

                FieldSplitter();
                DrawBehaviourField(ref defaultRiggedScript, "Animated Behaviour", ref defaultRiggedBehaviourEnabled);
                FieldSplitter();
                DrawBehaviourField(ref defaultVehicleScript, "Vehicle", ref defaultVehicleBehaviourEnabled);
                FieldSplitter();
                DrawBehaviourField(ref defaultFlyingScript, "Flying Vehicle", ref defaultFlyingVehicleBehaviourEnabled);
                FieldSplitter();
                DrawBehaviourField(ref defaultStaticScript, "Static", ref defaultStaticBehaviourEnabled);
                FieldSplitter();
                DrawBehaviourField(ref defaultShaderScript, "Shader", ref defaultShaderBehaviourEnabled);
                GUI.enabled = true;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All"))
                {
                    defaultRiggedBehaviourEnabled = true;
                    defaultVehicleBehaviourEnabled = true;
                    defaultFlyingVehicleBehaviourEnabled = true;
                    defaultStaticBehaviourEnabled = true;
                    defaultShaderBehaviourEnabled = true;

                }
                if (GUILayout.Button("Deselect All"))
                {
                    defaultRiggedBehaviourEnabled = false;
                    defaultVehicleBehaviourEnabled = false;
                    defaultFlyingVehicleBehaviourEnabled = false;
                    defaultStaticBehaviourEnabled = false;
                    defaultShaderBehaviourEnabled = false;
                }
                GUILayout.EndHorizontal();
            }
            #endregion

            #region GridSettings
            DrawerButton(ref showGridOptionsDrawer, "Grid Options");
            if (showGridOptionsDrawer)
            {
                EditorGUILayout.HelpBox("Change way grid placement works.", MessageType.Info);
                FieldSplitter();
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(showGridHandles ? "Grid Gizmos: On" : "Grid Gizmos: Off"))
                {
                    showGridHandles = !showGridHandles;
                    SceneView.RepaintAll();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                FieldSplitter();
                CustomObjectField(ref SimpleGrid.origin, "Grid Origin");
                FieldSplitter();
                CustomObjectField(ref SimpleGrid.cellWidth, "Cell Width");
                FieldSplitter();
                CustomObjectField(ref SimpleGrid.cellCount, "Grid Width");
            }
            #endregion
        }

        private void DrawerTitle(string str)
        {
            GUILayout.Label(str,
                new GUIStyle(HeaderLabelStyle)
                {
                    font = GetPoppinsFont(PoppinsStyle.Medium),
                    fontSize = 14,
                    fontStyle = FontStyle.Normal,
                    normal = new GUIStyleState() { textColor = EditorStyles.label.normal.textColor }
                });
        }
        private void DrawerButton(ref bool buttonActive, string label)
        {
            if (GUILayout.Button(label, buttonActive ? ButtonActiveStyle : ButtonInactiveStyle, GUILayout.Height(20)))
            {
                buttonActive = !buttonActive;
            }
        }
        private void FieldSplitter()
        {
            EditorGUILayout.Space(5);
        }


        private void DrawBehaviourField(ref MonoScript monoscript, string label, ref bool isActive)
        {

            //FB: Unresolved ExitGUIException that is a bug from unity gotta be caught here.
            try
            {
                GUILayout.BeginHorizontal();
                isActive = EditorGUILayout.Toggle(isActive, GUILayout.Width(20));
                GUI.enabled = isActive;
                monoscript = EditorGUILayout.ObjectField(label, monoscript, typeof(MonoScript), true) as MonoScript;
                GUILayout.EndHorizontal();
            }
            catch (UnityEngine.ExitGUIException)
            {
                //We suppress this because unity hasn't fixed this bug after 10 years.
                //throw;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            GUI.enabled = true;

        }
        private void CustomObjectField(ref Transform transform, string label, ref bool fieldEnabled)
        {
            GUILayout.BeginHorizontal();
            fieldEnabled = EditorGUILayout.Toggle(fieldEnabled, GUILayout.Width(20));
            GUI.enabled = fieldEnabled;
            transform = EditorGUILayout.ObjectField(label, transform, typeof(Transform), true) as Transform;
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        private void CustomObjectField(ref Vector3 vector, string label, ref bool fieldEnabled)
        {
            GUILayout.BeginHorizontal();
            fieldEnabled = EditorGUILayout.Toggle(fieldEnabled, GUILayout.Width(20));
            GUI.enabled = fieldEnabled;
            vector = EditorGUILayout.Vector3Field(label, vector);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        private void CustomObjectField(ref Vector3 vector, string label)
        {
            GUILayout.BeginHorizontal();
            vector = EditorGUILayout.Vector3Field(label, vector);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        private void CustomObjectField(ref float value, string label, ref bool fieldEnabled)
        {
            GUILayout.BeginHorizontal();
            fieldEnabled = EditorGUILayout.Toggle(fieldEnabled, GUILayout.Width(20));
            GUI.enabled = fieldEnabled;
            value = EditorGUILayout.FloatField(label, value);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        private void CustomObjectField(ref float value, string label)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.FloatField(label, value);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        private void CustomObjectField(ref int value, string label)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.IntField(label, value);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
        protected void DrawFilters()
        {
            GUILayout.Label("FILTER", new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.MiddleCenter });
            GUILayout.BeginHorizontal();
            DrawDropdown(new Vector3(0, 20), CategoryFilter, currentCategory, "CATEGORY");
            DrawDropdown(new Vector3(0, 20), AnimationFilter, currentAnimationFilter, "ANIMATED");
            DrawDropdown(new Vector3(0, 20), SortingFilter, currentSortingMethod, "SORT BY");
            GUILayout.EndHorizontal();
        }

        protected void DrawBrowserCard(List<SearchResult> resultArray, float columnCoord, float rowCoord, float buttonWidth, float buttonHeight, int searchIndex, float resultScaleMultiplier)
        {
            try
            {
                Event e = Event.current;
                // Set result data
                var result = resultArray[searchIndex];
                var displayThumbnail = result.Thumbnail;
                if (displayThumbnail == null)
                {
                    displayThumbnail = BlackAnythingGlobeLogo;
                }
                var modelName = new GUIContent(result.DisplayName);
                var authorName = new GUIContent(result.data.author);
                var cardRect = new Rect(columnCoord, rowCoord, buttonWidth, buttonHeight);

                // Initialize padding and sizing 
                var voteIconSizeX = Mathf.Max(BaseFilledHeart.width, BaseFilledHeart.width) / 2.5f * resultScaleMultiplier;
                var voteIconSizeY = Mathf.Max(BaseFilledHeart.height, BaseFilledHeart.height) / 2.5f * resultScaleMultiplier;

                var infoPaddingX = voteIconSizeX / 2f;
                var infoPaddingY = voteIconSizeY / 2f;

                //Draw elements
                var infoBackdropRect = new Rect(cardRect.x, cardRect.yMax - buttonWidth, buttonWidth, buttonWidth);
                GUI.DrawTexture(infoBackdropRect, StateCardBackdrop.activeTexture);

                var thumbnailBackdropRect = new Rect(cardRect.x, cardRect.y, buttonWidth, buttonHeight * 0.75f);

                if (GUI.Button(thumbnailBackdropRect, new GUIContent()))
                {
                    var inputParams = new List<RequestParam>();

                    if (customPositionFieldEnabled) inputParams.Add(RequestParameters.Position(objectPosition));
                    if (customRotationFieldEnabled) inputParams.Add(RequestParameters.Rotation(objectRotation));
                    if (customScaleMultiplierEnabled) inputParams.Add(RequestParameters.ScaleMultiplier(objectScaleMultiplier));
                    if (customParentTransformEnabled) inputParams.Add(RequestParameters.Parent(objectParentTransform));
                    inputParams.Add(RequestParameters.PlaceOnGrid(gridPlacementEnabled));
                    inputParams.Add(RequestParameters.Behaviours(DefaultBehavioursArray));
                    inputParams.Add(RequestParameters.IsAnimated(!makeStatic));
                    AnythingMaker.Make(result.data.name, inputParams.ToArray());
                }


                GUI.DrawTexture(thumbnailBackdropRect, BaseCardThumbnailBackdrops[searchIndex % BaseCardThumbnailBackdrops.Length], ScaleMode.ScaleAndCrop);
                GUI.DrawTexture(thumbnailBackdropRect, displayThumbnail, ScaleMode.ScaleAndCrop);
                if (cardRect.Contains(e.mousePosition))
                {
                    if (e.button == 0 && e.isMouse)
                    {
                        GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
                    }
                    GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
                }
                DrawVoteButton(result, ref infoBackdropRect, voteIconSizeX, voteIconSizeY, infoPaddingX, infoPaddingY, out var voteRect);
                DrawVoteCountLabel(infoPaddingX, voteRect, result.data.voteScore, resultScaleMultiplier);
                DrawListIcon(result, infoBackdropRect, infoPaddingX, infoPaddingY, resultScaleMultiplier);

                DrawAuthorIconBackground(ref infoBackdropRect, ref thumbnailBackdropRect, infoPaddingX, infoPaddingY, out var authorIconRect, resultScaleMultiplier);
                DrawModelNameLabel(modelName, infoPaddingX, cardRect.xMax, ref authorIconRect, out var modelNameLabelRect, resultScaleMultiplier);
                DrawAuthorLabel(authorName, infoPaddingX, cardRect.xMax, authorIconRect, modelNameLabelRect, resultScaleMultiplier);

                if (result.isAnimated) DrawAnimationStatusIcon(thumbnailBackdropRect, infoPaddingX, infoPaddingY, resultScaleMultiplier);
                FindCategoryIcon(CreationSearchCategory.MODELS, searchIndex, out var categoryIcon);
            }
            catch
            {
                Debug.Log("issue drawing browser card: " + searchIndex);
            }
           
        }
        #region Browser Card Draw Methods
        protected void DrawVoteButton(SearchResult result, ref Rect infoBackdropRect, float voteIconSizeX, float voteIconSizeY, float infoPaddingX, float infoPaddingY, out Rect voteRect)
        {
            voteRect = new Rect(infoBackdropRect.x + infoPaddingX, infoBackdropRect.yMax - infoPaddingY - voteIconSizeY, voteIconSizeX, voteIconSizeY);
            if (GUI.Button(voteRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(result.data.userVote == "upvote" ? StateHeartIcon.activeTexture : StateHeartIcon.inactiveTexture), hover = SetStyleState(StateHeartIcon.hoverTexture) }))
            {
                UserVoteProcessor.FlipUserVote(result, this);
            }
        }

        protected void DrawVoteCountLabel(float infoPaddingX, Rect voteRect, int voteCount, float scaleMultiplier)
        {
            var voteStyle = new GUIStyle(VoteStyle) { fontSize = (int)(12 * scaleMultiplier) };
            var voteContent = new GUIContent(TruncateNumber(voteCount));
            var voteLabelWidth = voteStyle.CalcSize(voteContent).x;
            var voteLabelRect = new Rect(voteRect.xMax + (infoPaddingX / 2), voteRect.y, voteLabelWidth, voteRect.height);
            GUI.Label(voteLabelRect, voteContent, voteStyle);
        }

        protected void DrawAuthorIconBackground(ref Rect infoBackdropRect, ref Rect thumbnailBackdropRect, float infoPaddingX, float infoPaddingY, out Rect userIconRect, float scaleMultiplier)
        {
            var userIconSize = BaseUserIcon.width / 2.5f * scaleMultiplier;
            userIconRect = new Rect(infoBackdropRect.x + infoPaddingX, thumbnailBackdropRect.yMax + infoPaddingY / 1.5f, userIconSize, userIconSize);
            GUI.DrawTexture(userIconRect, TintedUserIcon);
        }

        protected void DrawListIcon(SearchResult result, Rect infoBackdropRect, float infoPaddingX, float infoPaddingY, float scaleMultiplier)
        {
            //Draw List Icon 
            var listIconSizeX = BaseCollectionIcon.width / 2.5f * scaleMultiplier;
            var listIconSizeY = BaseCollectionIcon.height / 2.5f * scaleMultiplier;
            var listRect = new Rect(infoBackdropRect.xMax - infoPaddingX - listIconSizeX, infoBackdropRect.yMax - infoPaddingY - listIconSizeY, listIconSizeX, listIconSizeY);
            if (GUI.Button(listRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateCollectionIcon.activeTexture), hover = SetStyleState(StateCollectionIcon.hoverTexture) }))
            {
                AnythingSubwindow.OpenWindow("Add to Collection", new Vector2(300, 450), DrawCollectionWindow, position, result);
            }
        }

        protected void DrawAnimationStatusIcon(Rect thumbnailBackdropRect, float infoPaddingX, float infoPaddingY, float scaleMultiplier)
        {
            //Top Left Animated Icon Indicator
            var animatedIconSizeX = BaseAnimatedIcon.width / 2.5f * scaleMultiplier;
            var animatedIconSizeY = BaseAnimatedIcon.height / 2.5f * scaleMultiplier;
            var animatedRect = new Rect(thumbnailBackdropRect.x + infoPaddingX, thumbnailBackdropRect.y + infoPaddingY, animatedIconSizeX, animatedIconSizeY);
            GUI.DrawTexture(animatedRect, BaseAnimatedIcon);
        }

        protected static void FindCategoryIcon(CreationSearchCategory category, int searchIndex, out Texture2D categoryIcon)
        {
            categoryIcon = category switch
            {
                CreationSearchCategory.MODELS => BaseCardObjectIcon,
                CreationSearchCategory.WORLDS => BaseCardWorldIcon,
                CreationSearchCategory.LIGHTING => BaseCardLightingIcon,
                CreationSearchCategory.COLLECTION => BaseCardCollectionIcon,
                _ => null
            };
        }

        protected void DrawCategoryIcon(Rect thumbnailBackdropRect, float infoPaddingX, float infoPaddingY, Texture2D categoryIcon, float scaleMultiplier)
        {
            var categoryIconSizeX = categoryIcon.width / 2.5f * scaleMultiplier;
            var categoryIconSizeY = categoryIcon.height / 2.5f * scaleMultiplier;
            var categoryRect = new Rect(thumbnailBackdropRect.xMax - infoPaddingX - categoryIconSizeX, thumbnailBackdropRect.y + infoPaddingY, categoryIconSizeX, categoryIconSizeY);
            if (categoryIcon != null) GUI.DrawTexture(categoryRect, categoryIcon);
        }

        protected void DrawModelNameLabel(GUIContent modelName, float infoPaddingX, float buttonRightEdge, ref Rect userIconRect, out Rect modelNameLabelRect, float scaleMultiplier)
        {
            var modelNameStyle = new GUIStyle(ModelNameStyle) { fontSize = (int)(12 * scaleMultiplier) };
            var modelNameXPos = userIconRect.xMax + (infoPaddingX / 2);
            var modelNameLabelWidth = Mathf.Min(modelNameStyle.CalcSize(modelName).x, buttonRightEdge - modelNameXPos);
            modelNameLabelRect = new Rect(modelNameXPos, userIconRect.y, modelNameLabelWidth, userIconRect.height / 2);
            GUI.Label(modelNameLabelRect, modelName, modelNameStyle);
        }

        protected void DrawAuthorLabel(GUIContent authorName, float infoPaddingX, float buttonRightEdge, Rect userIconRect, Rect modelNameLabelRect, float scaleMultiplier)
        {
            //Draw Author Label
            var authorStyle = new GUIStyle(AuthorNameStyle) { fontSize = (int)(10 * scaleMultiplier) };
            var authorNameXPos = userIconRect.xMax + (infoPaddingX / 2);
            var authorNameLabelWidth = Mathf.Min(authorStyle.CalcSize(authorName).x, buttonRightEdge - authorNameXPos);
            var authorNameLabelRect = new Rect(authorNameXPos, modelNameLabelRect.yMax, authorNameLabelWidth, userIconRect.height / 2);
            GUI.Label(authorNameLabelRect, authorName, authorStyle);
        }
        #endregion
        protected void DrawGrid<T>(List<T> results, int cellCount, float cellWidth, float cellHeight, Action<List<T>, float, float, float, float, int, float> drawCellFunction, float scaleMultiplier = 1f)
        {
            if (cellCount == 0) return;

            var internalMultiplier = 1.5f;
            var buttonWidth = cellWidth * internalMultiplier * scaleMultiplier;
            var buttonHeight = cellHeight * internalMultiplier * scaleMultiplier;
            var aspectRatio = cellHeight / cellWidth;

            var verticalMargin = 5 * internalMultiplier;
            var horizontalMargin = 5 * internalMultiplier;
            float scrollBarAllowance = 6;
            var buttonWidthWithMargin = buttonWidth + horizontalMargin;
            var resultsPerLine = Mathf.Floor((position.width - horizontalMargin) / buttonWidthWithMargin);
            if (resultsPerLine == 0)
            {
                resultsPerLine = 1;
                var scalingFix = scaleMultiplier;
                if (buttonWidth > position.width)
                {
                    scalingFix = (position.width / cellWidth) / internalMultiplier;
                    buttonWidth = position.width;
                    buttonHeight = buttonWidth * aspectRatio;
                    buttonWidthWithMargin = buttonWidth + horizontalMargin;
                }
                scaleMultiplier = scalingFix;
            }
            var rows = (int)Math.Ceiling(cellCount / resultsPerLine);
            var actualBlockWidth = (resultsPerLine * buttonWidthWithMargin) + horizontalMargin;
            var outerRemainder = position.width - actualBlockWidth;
            var remainderMargin = outerRemainder / 2;

            var cardIndex = 0;

            var lastRect = GUILayoutUtility.GetLastRect();
            var gridArea = new Rect(0, lastRect.yMax, position.width + scrollBarAllowance, (buttonHeight * rows) + (verticalMargin * rows));
            var view = new Rect(0, lastRect.yMax, position.width, position.height - lastRect.yMax);
            newScrollPosition = GUI.BeginScrollView(view, newScrollPosition, gridArea, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            if (copiedToKeyboard)
            {
                if (!copiedRect.Contains(Event.current.mousePosition))
                {
                    copiedToKeyboard = false;
                }
            }
            var scrollViewRect = new Rect(new Vector2(view.x, view.y + newScrollPosition.y), view.size);


            //It through rows and draw 
            for (var yPos = 0; yPos < rows; yPos++)
            {

                var rowCoord = view.yMin + (yPos * buttonHeight) + (verticalMargin * yPos);

                if (rowCoord > scrollViewRect.yMax) continue;
                if (rowCoord+buttonHeight < scrollViewRect.yMin) continue;
                GUI.DrawTexture(lastRect, TintedUserIcon);
               // if (rowCoord * buttonHeight > gridArea.height) break;
                for (var xPos = 0; xPos < resultsPerLine; xPos++)
                {
                    var columnCoord = (xPos * buttonWidthWithMargin) + horizontalMargin + (remainderMargin - scrollBarAllowance);
                    var index = (yPos * (int)resultsPerLine) + xPos;

                    if (results.Count>index)
                    {
                        drawCellFunction(results, columnCoord, rowCoord, buttonWidth, buttonHeight,(yPos*(int)Mathf.FloorToInt(resultsPerLine)) + xPos, scaleMultiplier);
                    }
                    else
                    {
                        break;
                    }
                    cardIndex++;
                }

            }
            GUI.EndScrollView();

            scrollView = GUILayoutUtility.GetLastRect();
        }


        Rect scrollView = new Rect(0, 0, 0, 0);

        string[] existingCollections;
        string collectionSearchTerm;
        Vector2 collectionScrollPosition;

        protected void DrawCollectionWindow(AnythingEditor window, SearchResult result)
        {
            CollectionProcessor.GetCollectionNames(AssignCollectionNames, this);
            void AssignCollectionNames(string[] results)
            {
                existingCollections = results;
            }

            GUILayout.Label("Add to a pre-existing collection?", new GUIStyle(HeaderLabelStyle) { fontSize = 12 });
            if (existingCollections != null)
            {
                collectionScrollPosition = GUILayout.BeginScrollView(collectionScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(window.position.height / 2.5f));
                foreach (string collectionName in existingCollections)
                {
                    if (GUILayout.Button($"{collectionName}", ButtonInactiveStyle))
                    {
                        CollectionProcessor.AddToCollection(result, collectionName, this);
                        window.Close();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.Label("...or make a new collection?", new GUIStyle(HeaderLabelStyle) { fontSize = 12 });
            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            collectionSearchTerm = GUILayout.TextField(collectionSearchTerm);
            GUILayout.Space(8);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Create Collection", ButtonInactiveStyle))
            {
                CollectionProcessor.AddToCollection(result, collectionSearchTerm, this);
                collectionSearchTerm = "";
                window.Close();
            }
        }

        protected void SetupResetEditorWindow(ResetMode resetMode)
        {
            editorResetMode = resetMode;
            AnythingSubwindow.OpenWindow($"Reset {editorResetMode}?", new Vector2(300f, 150f), DrawResetEditorWindow, position);
        }

        private ResetMode editorResetMode;
        protected void DrawResetEditorWindow(AnythingEditor window)
        {
            string inlineResetText = editorResetMode switch
            {
                ResetMode.Scene => "the scene",
                ResetMode.Creator => "the creator",
                ResetMode.All => "everything",
                _ => ""
            };

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Are you sure you want to reset {inlineResetText}?", new GUIStyle(HeaderLabelStyle) { fontSize = 16 });

            if (GUILayout.Button($"Reset {editorResetMode}", ButtonInactiveStyle))
            {
                ResetAnythingWorld(editorResetMode);
                window.Close();
            }
            GUILayout.FlexibleSpace();
        }
        #endregion Editor Drawing

        #region Helper Functions

        protected enum ResetMode
        {
            Scene, Creator, All
        }

        protected virtual void ResetAnythingWorld(ResetMode resetMode)
        {
            if (resetMode != ResetMode.Scene)
            {
                resultThumbnailMultiplier = 1f;
                searchModeFailReason = "";
                searchMode = SearchMode.IDLE;

                currentAnimationFilter = AnimatedDropdownOption.BOTH;
                currentCategory = CategoryDropdownOption.ALL;
                currentSortingMethod = SortingDropdownOption.MostRelevant;

                objectParentTransform = null;
                objectPosition = Vector3.zero;
                objectRotation = Vector3.zero;
                objectScaleMultiplier = 1f;
                transformFieldsEnabled = false;
                customParentTransformEnabled = false;
                customPositionFieldEnabled = false;
                customRotationFieldEnabled = false;
                customScaleMultiplierEnabled = false;

                makeStatic = false;

                addDefaultBehaviour = false;
                defaultVehicleBehaviourEnabled = false;
                defaultFlyingVehicleBehaviourEnabled = false;
                defaultRiggedBehaviourEnabled = false;
                defaultStaticBehaviourEnabled = false;
                defaultShaderBehaviourEnabled = false;

                addCustomBehaviours = false;
                customBehaviourDictionary = new Dictionary<string, UnityEngine.Object>();

                showDefaultBehavioursDrawer = false;
                showTransformDrawer = false;
                showGeneralDrawer = false;
                gridPlacementEnabled = false;
                transformSettingsActive = false;

                AssignDefaultBehavioursFromScriptable();
            }
        }

        public void UpdateSearchResults(SearchResult[] results, string onEmpty)
        {
            searchResults = new List<SearchResult>();
            searchMode = SearchMode.SUCCESS;

            if (results == null || results.Length == 0)
            {
                searchMode = SearchMode.FAILURE;
                searchModeFailReason = onEmpty;
                return;
            }

            if (results.Length > 0)
            {
                searchResults = results.ToList();
                FilterSearchResult(searchResults);
            }
        }
        public void UpdateSearchResults(List<SearchResult> results, string onEmpty)
        {
            searchResults = new List<SearchResult>();
            searchMode = SearchMode.SUCCESS;

            if (results == null || results.Count == 0)
            {
                searchMode = SearchMode.FAILURE;
                searchModeFailReason = onEmpty;
                return;
            }

            if (results.Count > 0)
            {
                searchResults = results;
                FilterSearchResult(searchResults);
            }
        }
        public void UpdateSearchResults(ref List<SearchResult> unfiltered, ref List<SearchResult> filtered, string onEmpty)
        {
            unfiltered = new List<SearchResult>();

            if (unfiltered == null || unfiltered.Count == 0)
            {
                return;
            }

            if (unfiltered.Count > 0)
            {
                searchResults = unfiltered.ToList();
                filtered = FilterAndReturnResults(searchResults);
            }
        }
        public List<SearchResult> FilterAndReturnResults(List<SearchResult> results)
        {
            var filtered = FilterByAnimation(results);
            filtered = FilterByCategory(filtered);
            filtered = SortResults(filtered);
            filteredResults = filtered;
            return filteredResults;
        }
        public void FilterSearchResult(List<SearchResult> results)
        {
            var filtered = FilterByAnimation(results);
            filtered = FilterByCategory(filtered);
            filtered = SortResults(filtered);
            filteredResults = filtered;
            if (filteredResults == null)
            {
                searchMode = SearchMode.FAILURE;
                searchModeFailReason = "We couldn't find any models matching those filters.";
            }

            Repaint();
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private List<SearchResult> FilterByCategory(List<SearchResult> results)
        {
            var categoryFilter = new List<SearchResult>();

            if (currentCategory == CategoryDropdownOption.ALL) return results;

            string categoryWord = currentCategory.ToString().ToLower();
            categoryFilter = (from result in results where result.data.themeCategories.Contains(categoryWord) select result).ToList();
            return categoryFilter;
        }

        private List<SearchResult> FilterByAnimation(List<SearchResult> results)
        {
            List<SearchResult> animationFilter = new List<SearchResult>();
            switch (currentAnimationFilter)
            {
                case AnimatedDropdownOption.BOTH:
                    animationFilter = results;
                    break;
                case AnimatedDropdownOption.ANIMATED:
                    animationFilter = (from result in results where result.isAnimated select result).ToList();
                    break;
                case AnimatedDropdownOption.STILL:
                    animationFilter = (from result in results where !result.isAnimated select result).ToList();
                    break;
            }
            return animationFilter;
        }

        private List<SearchResult> SortResults(List<SearchResult> results)
        {
            List<SearchResult> sortedResults = new List<SearchResult>();
            switch (currentSortingMethod)
            {
                case SortingDropdownOption.MostRelevant:
                    sortedResults = results;
                    break;
                /* case SortingDropdownOption.MostPopular:
                     sortedResults = (from result in results orderby result.data.popularity select result).ToList();
                     break;
                */
                case SortingDropdownOption.MostLiked:
                    sortedResults = (from result in results orderby result.data.voteScore select result).ToList();
                    sortedResults.Reverse();
                    break;
                case SortingDropdownOption.MyList:
                    sortedResults = (from result in results where result.data.userVote == "upvote" select result).ToList();
                    break;
                case SortingDropdownOption.AtoZ:
                    sortedResults = (from result in results orderby result.data.name select result).ToList();
                    break;
                case SortingDropdownOption.ZtoA:
                    sortedResults = (from result in results orderby result.data.name select result).ToList();
                    sortedResults.Reverse();
                    break;
            }

            return sortedResults;
        }

        protected string TruncateNumber(int number)
        {
            switch (number)
            {
                case var _ when number >= 100000000:
                    return (number / 1000000).ToString("#,0M");
                case var _ when number >= 10000000:
                    return (number / 1000000).ToString("0.#") + "M";
                case var _ when number >= 100000:
                    return (number / 1000).ToString("#,0K");
                case var _ when number >= 10000:
                    return (number / 1000).ToString("0.#") + "K";
                default:
                    return number.ToString("#,0");
            };
        }
        #endregion Helper Functions
    }

    public class AnythingSubwindow : AnythingEditor
    {
        protected static Rect windowPosition;
        protected static Rect callingWindowScreenPosition;

        protected static bool invokeWithParameter;
        protected static bool resetWindowPosition = true;

        protected static string windowTitle;
        protected static Vector2 windowSize;

        protected static Action<AnythingEditor> windowAction;
        protected static Action<AnythingEditor, SearchResult> windowActionSR;
        protected static SearchResult searchResult;

        public static void OpenWindow(string title, Vector2 size, Action<AnythingEditor, SearchResult> guiAction, Rect callingWindow, SearchResult result)
        {
            callingWindowScreenPosition = GUIUtility.GUIToScreenRect(callingWindow);
            windowTitle = title;
            windowSize = size;
            windowActionSR = guiAction;
            searchResult = result;

            invokeWithParameter = true;
            ShowWindow();
        }

        public static void OpenWindow(string title, Vector2 size, Action<AnythingEditor> guiAction, Rect callingWindow)
        {
            callingWindowScreenPosition = GUIUtility.GUIToScreenRect(callingWindow);
            windowTitle = title;
            windowSize = size;
            windowAction = guiAction;

            invokeWithParameter = false;
            ShowWindow();
        }

        protected static void ShowWindow()
        {
            var window = GetWindow<AnythingSubwindow>(true);

            window.titleContent = new GUIContent(windowTitle);
            window.minSize = window.maxSize = windowSize;

            if (resetWindowPosition)
            {
                resetWindowPosition = false;
                windowPosition = GUIUtility.ScreenToGUIRect(new Rect(callingWindowScreenPosition.x + ((callingWindowScreenPosition.width - window.minSize.x) / 2), callingWindowScreenPosition.y + ((callingWindowScreenPosition.height - window.minSize.y) / 2), 0, 0));
            }
            else
            {
                windowPosition = window.position;
            }

            window.position = windowPosition;
        }

        protected new void OnGUI()
        {
            base.OnGUI();
            if (invokeWithParameter) windowActionSR.Invoke(this, searchResult);
            else windowAction.Invoke(this);

        }

        protected void OnDestroy()
        {
            resetWindowPosition = true;
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
        }
    }
}
