// =============================================================================
// IUISettingsConsumer.cs   —   Assets/Scripts/UI/
//
// Any MonoBehaviour that displays text or UI art implements this interface.
// When the player saves or previews a settings change, OptionsManager calls
// ApplySettingsToScene() which finds every consumer via FindObjectsByType
// and notifies them.
//
// Example — a DialogueBox MonoBehaviour:
//
//   public class DialogueBox : MonoBehaviour, IUISettingsConsumer
//   {
//       public Image border, box, gradient;
//       public Text  dialogueText;
//       public Shadow textShadow;
//
//       public void OnSettingsChanged(UISettings s)
//       {
//           var lib = UIAssetLibrary.Instance;
//           if (border)         border.sprite           = lib.GetBorder(s.textBorderIndex);
//           if (box)            { box.sprite            = lib.GetBox(s.textBoxIndex);
//                                 box.color             = new Color(s.textBoxColor.r,
//                                                                   s.textBoxColor.g,
//                                                                   s.textBoxColor.b,
//                                                                   s.textBoxTransparency); }
//           if (gradient)       { gradient.sprite       = lib.GetGradient(s.textBorderIndex);
//                                 gradient.color        = new Color(s.gradientColor.r,
//                                                                   s.gradientColor.g,
//                                                                   s.gradientColor.b,
//                                                                   s.gradientEnabled ? s.gradientTransparency : 0f); }
//           if (dialogueText)   { dialogueText.font     = lib.GetFont(s.fontIndex);
//                                 dialogueText.color    = s.textColor; }
//           if (textShadow)     { textShadow.effectColor = new Color(s.textShadowColor.r,
//                                                                     s.textShadowColor.g,
//                                                                     s.textShadowColor.b,
//                                                                     s.textShadowTransparency); }
//       }
//   }
// =============================================================================

public interface IUISettingsConsumer
{
    /// <summary>
    /// Called by OptionsManager.ApplySettingsToScene() whenever the player
    /// changes or saves a setting.  Also called once on scene load so every
    /// dialogue box, HUD element, etc., starts with the correct appearance.
    /// </summary>
    void OnSettingsChanged(UISettings settings);
}
