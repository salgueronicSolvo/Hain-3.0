using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AnythingWorld.Editor
{
    public class AnythingEditor : EditorWindow
    {
        #region Fields
        #region Fonts
        private static Font poppinsRegular;
        protected static Font POPPINS_REGULAR
        {
            get
            {
                if (poppinsRegular == null)
                {
                    poppinsRegular = (Font)Resources.Load("Fonts/Poppins/Poppins-Regular", typeof(Font));
                }
                return poppinsRegular;
            }
        }

        private static Font poppinsMedium;
        protected static Font POPPINS_MEDIUM
        {
            get
            {
                if (poppinsMedium == null)
                {
                    poppinsMedium = (Font)Resources.Load("Fonts/Poppins/Poppins-Medium", typeof(Font));
                }
                return poppinsMedium;
            }
        }

        private static Font poppinsBold;
        protected static Font POPPINS_BOLD
        {
            get
            {
                if (poppinsBold == null)
                {
                    poppinsBold = (Font)Resources.Load("Fonts/Poppins/Poppins-Bold", typeof(Font));
                }
                return poppinsBold;
            }
        }

        public enum PoppinsStyle
        {
            Regular,
            Bold,
            Medium
        }
        #endregion Fonts
        #region Textures
        private Texture2D baseGradientBanner;
        protected Texture2D BaseGradientBanner
        {
            get
            {
                if (baseGradientBanner == null)
                {
                    baseGradientBanner = Resources.Load("Editor/Shared/gradientRectangleBW") as Texture2D;
                }
                return baseGradientBanner;
            }
        }

        private Texture2D baseAnythingGlobeLogo;
        protected Texture2D BaseAnythingGlobeLogo
        {
            get
            {
                if (baseAnythingGlobeLogo == null)
                {
                    baseAnythingGlobeLogo = Resources.Load("Editor/Shared/whiteGlobeLogo") as Texture2D;
                }
                return baseAnythingGlobeLogo;
            }
        }

        private Texture2D baseDropdownArrow;
        protected Texture2D BaseDropdownArrow
        {
            get
            {
                if (baseDropdownArrow == null)
                {
                    baseDropdownArrow = Resources.Load("Editor/Shared/dropdownArrow") as Texture2D;
                }
                return baseDropdownArrow;
            }
        }

        private Texture2D tintedDropdownArrow;
        protected Texture2D TintedDropdownArrow
        {
            get
            {
                if (tintedDropdownArrow == null)
                {
                    tintedDropdownArrow = TintTextureToEditorTheme(BaseDropdownArrow, Color.white, Color.black);
                }
                return tintedDropdownArrow;
            }
            set => tintedDropdownArrow = value;
        }
        #endregion Textures
        #region Styles

        protected static GUIStyle HeaderLabelStyle;
        protected static GUIStyle BodyLabelStyle;
        protected static GUIStyle ButtonStyle;
        protected static GUIStyle DropdownStyle;
        protected static GUIStyle InputFieldStyle;
        protected static GUIStyle ToggleStyle;
        #endregion Styles
        protected static bool editorInitialized = false;
        #endregion Fields

        #region Unity Messages
        protected void OnInspectorUpdate()
        {
            //Repaint();
        }

        protected void Awake()
        {
            editorInitialized = false;
            InitializeResources();
        }

        protected void OnGUI()
        {
            if (!editorInitialized)
            {
                if (!InitializeResources())
                {
                    return;
                }
            }
            EditorApplication.playModeStateChanged -= InitializeResources;
            EditorApplication.playModeStateChanged += InitializeResources;
        }
        #endregion Unity Messages

        #region Functions
        #region Initialization

        /// <summary>
        /// Calls the <see cref="InitializeCustomStyles"/> function.
        /// </summary>
        public bool InitializeResources()
        {
            editorInitialized = false;

            if (InitializeCustomStyles())
            {
                editorInitialized = true;
                return true;
            }
            return false;
        }

        public void InitializeResources(PlayModeStateChange state)
        {
            editorInitialized = InitializeCustomStyles();

            if(state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode) Repaint();
        }

        private bool InitializeCustomStyles()
        {
            try
            {
                DefineCustomStyles();
            }
            catch (Exception e)
            {
                if (AnythingSettings.DebugEnabled) Debug.LogError($"Error initializing custom styles with error: \n{e}");
                return false;
            }
            return true;
        }

        protected virtual void DefineCustomStyles()
        {
            HeaderLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                font = GetPoppinsFont(PoppinsStyle.Bold),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                margin = UniformRectOffset(10),
                wordWrap = true
            };

            BodyLabelStyle = new GUIStyle(EditorStyles.label)
            {
                font = GetPoppinsFont(PoppinsStyle.Regular),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                wordWrap = true
            };

            ButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                stretchHeight = true,
                fixedHeight = 30,
                font = GetPoppinsFont(PoppinsStyle.Bold),
                fontSize = 12,
                margin = UniformRectOffset(10)
            };

            DropdownStyle = new GUIStyle
            {
                stretchHeight = true,
                font = GetPoppinsFont(PoppinsStyle.Bold),
                fontSize = 12,
                normal =
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                },
                margin = new RectOffset(5, 5, 0, 0),
                padding = new RectOffset(10, 10, 0, 0)
            };

            InputFieldStyle = new GUIStyle
            {
                font = GetPoppinsFont(PoppinsStyle.Medium),
                fontSize = 14,
                margin = UniformRectOffset(10),
                padding =
                {
                    top = 0
                },
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                },
                contentOffset = new Vector2(16, 0),
                clipping = TextClipping.Clip
            };

            ToggleStyle = new GUIStyle(EditorStyles.toggle)
            {
                fixedHeight = 30,
                imagePosition = ImagePosition.ImageLeft,
                font = GetPoppinsFont(PoppinsStyle.Regular),
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(InputFieldStyle.margin.left, 0, 0, 0),
                padding = new RectOffset(20, 0, 0, 2)
            };
        }
        #endregion Initialization

        #region Helper Functions
        protected struct DropdownOption
        {
            public string label;
            public GenericMenu.MenuFunction function;
            public GenericMenu.MenuFunction2 function2;
            public object dataEndpoint;
        }

        protected void DrawDropdown(Vector2 size, DropdownOption[] options, object dataMetric, string dropdownTitle = "")
        {
            EditorGUILayout.BeginVertical();
            if (!string.IsNullOrEmpty(dropdownTitle))
            {
                var titleHeight = BodyLabelStyle.CalcSize(new GUIContent(dropdownTitle)).y;

                var titleRect = GUILayoutUtility.GetRect(size.x, titleHeight, BodyLabelStyle, GUILayout.Height(titleHeight));
                GUI.Label(titleRect, new GUIContent(dropdownTitle), new GUIStyle(BodyLabelStyle) { fontSize = 12, margin = new RectOffset(10, 10, 0, 0), padding = new RectOffset(10, 10, 0, 0), contentOffset = new Vector2(6, 0) });
            }

            var dropdownRect = GUILayoutUtility.GetRect(size.x, size.y, DropdownStyle, GUILayout.Height(size.y));
            var padding = (size.y - BaseDropdownArrow.height) / 2;
            
            var dropdownStatus = EditorGUI.DropdownButton(dropdownRect, new GUIContent(options.FirstOrDefault(x => x.dataEndpoint.Equals(dataMetric)).label), FocusType.Passive, new GUIStyle(DropdownStyle) { fixedWidth = dropdownRect.width - BaseDropdownArrow.width, fixedHeight = size.y, clipping = TextClipping.Clip });
            DrawUILine(EditorGUIUtility.isProSkin ? new Color(0.341176471f, 0.345098039f, 0.349019608f) : 
                                                    new Color(0.894117647f, 0.898039216f, 0.890196078f), new Vector2(dropdownRect.xMin + 4, dropdownRect.yMax), dropdownRect.width - 8);

            var arrowRect = new Rect(dropdownRect.xMax - BaseDropdownArrow.width - padding, dropdownRect.y + padding, BaseDropdownArrow.width, BaseDropdownArrow.height);
            GUI.DrawTexture(arrowRect, TintedDropdownArrow);

            EditorGUILayout.EndVertical();

            if (!dropdownStatus) return;

            GenericMenu menu = new GenericMenu();

            foreach(var option in options)
            {
                AddMenuOption(menu, option.label, option.function, option.dataEndpoint.Equals(dataMetric));
            }

            menu.DropDown(dropdownRect);
        }

        protected void AddMenuOption(GenericMenu menu, string label, GenericMenu.MenuFunction function, bool activeCondition)
        {
            bool itemActive = activeCondition;
            menu.AddItem(new GUIContent(label.Replace("&", "and")), itemActive, function);
        }

        protected static RectOffset UniformRectOffset(int offset)
        {
            return new RectOffset(offset, offset, offset, offset);
        }

        protected static Font GetPoppinsFont(PoppinsStyle style)
        {
            Font chosenFont;
            switch (style)
            {
                case PoppinsStyle.Regular:
                    chosenFont = POPPINS_REGULAR;
                    break;
                case PoppinsStyle.Medium:
                    chosenFont = POPPINS_MEDIUM;
                    break;
                case PoppinsStyle.Bold:
                    chosenFont = POPPINS_BOLD;
                    break;
                default:
                    chosenFont = POPPINS_REGULAR;
                    break;
            }
            return chosenFont;
        }

        protected GUIStyleState SetStyleState(Texture2D background)
        {
            return new GUIStyleState() { background = background };
        }

        protected GUIStyleState SetStyleState(Color textColour)
        {
            return new GUIStyleState() { textColor = textColour };
        }

        protected GUIStyleState SetStyleState(Texture2D background, Color textColour)
        {
            return new GUIStyleState() { background = background, textColor = textColour };
        }

        protected void DrawUILine(Color color, Vector2 position, float width, int thickness = 1)
        {
            var rect = new Rect(position.x, position.y, width, thickness);
            EditorGUI.DrawRect(rect, color);
        }

        protected Color HexToColour(string hexCode)
        {
            hexCode = hexCode.ToUpper();

            if (hexCode.Length != 3 && hexCode.Length != 6)
            {
                return Color.black;
            }
            else
            {
                string r;
                string g;
                string b;

                int hexCodePrecision = hexCode.Length / 3;
                r = hexCode.Substring(0 * hexCodePrecision, hexCodePrecision);
                g = hexCode.Substring(1 * hexCodePrecision, hexCodePrecision);
                b = hexCode.Substring(2 * hexCodePrecision, hexCodePrecision);


                Color rgbColor = new Color(
                    int.Parse(r, System.Globalization.NumberStyles.HexNumber) / 255f,
                    int.Parse(g, System.Globalization.NumberStyles.HexNumber) / 255f,
                    int.Parse(b, System.Globalization.NumberStyles.HexNumber) / 255f);
                return rgbColor;
            }
        }
        protected Texture2D TintTextureToEditorTheme(Texture2D texture) => TintTexture(texture, EditorGUIUtility.isProSkin ? Color.white : Color.black);
        protected Texture2D TintTextureToEditorTheme(Texture2D texture, Color darkThemeTint, Color lightThemeTint) => TintTexture(texture, EditorGUIUtility.isProSkin ? darkThemeTint : lightThemeTint);
        protected Texture2D TintTexture(Texture2D untintedTexture, Color tintColour)
        {
            Color32[] untintedPixels = untintedTexture.GetPixels32();
            Color32[] tintedPixels = new Color32[untintedPixels.Length];

            Array.Copy(untintedPixels, tintedPixels, untintedPixels.Length);

            for (int i = 0; i < tintedPixels.Length; i++)
            {
                tintedPixels[i] = untintedPixels[i] * tintColour;
            }

            Texture2D tintedTexture = Instantiate(untintedTexture);
            tintedTexture.SetPixels32(tintedPixels);
            tintedTexture.Apply();
            return tintedTexture;
        }
        protected Texture2D TintTextureWhite(Texture2D untintedTexture, Color tintColour)
        {
            Color32[] untintedPixels = untintedTexture.GetPixels32();
            Color32[] tintedPixels = new Color32[untintedPixels.Length];

            Array.Copy(untintedPixels, tintedPixels, untintedPixels.Length);

            for (int i = 0; i < tintedPixels.Length; i++)
            {
                if (untintedPixels[i] == Color.white)
                {
                    tintedPixels[i] = untintedPixels[i] * tintColour;
                }
            }

            Texture2D tintedTexture = Instantiate(untintedTexture);
            tintedTexture.SetPixels32(tintedPixels);
            tintedTexture.Apply();
            return tintedTexture;
        }
        protected Texture2D TintGradient(Texture2D untintedGradient, Color tintColourA, Color tintColourB)
        {
            Color32[] untintedPixels = untintedGradient.GetPixels32();
            Color32[] tintedPixels = new Color32[untintedPixels.Length];

            Array.Copy(untintedPixels, tintedPixels, untintedPixels.Length);

            for (int i = 0; i < tintedPixels.Length; i++)
            {
                var maxRGB = Mathf.Max(untintedPixels[i].r, untintedPixels[i].g, untintedPixels[i].b) / 255f;
                var minRGB = Mathf.Min(untintedPixels[i].r, untintedPixels[i].g, untintedPixels[i].b) / 255f;
                tintedPixels[i] = Color.Lerp(tintColourA, tintColourB, (maxRGB + minRGB) / 2);
                tintedPixels[i].a = untintedPixels[i].a;
            }

            Texture2D tintedTexture = Instantiate(untintedGradient);
            tintedTexture.SetPixels32(tintedPixels);
            tintedTexture.Apply();
            return tintedTexture;
        }
        #endregion Helper Functions
        #endregion Functions
    }
}