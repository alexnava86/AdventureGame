using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ColorSwapper : MonoBehaviour
{
    public Color32[] primaryColors;
    [Range(-180, 180)] public int primaryHue;
    [Range(-1f, 1f)] public float primarySaturation;
    [Range(-1f, 1f)] public float primaryBrightness;
    public Color32[] secondaryColors;
    [Range(-180, 180)] public int secondaryHue;
    [Range(-1f, 1f)] public float secondarySaturation;
    [Range(-1f, 1f)] public float secondaryBrightness;
    public Color32[] accentColors;
    [Range(-180, 180)] public int accentHue;
    [Range(-1f, 1f)] public float accentSaturation;
    [Range(-1f, 1f)] public float accentBrightness;
    [Range(0, 2)] public int colorType;
    private Texture2D colorPalette;
    private Color[] spriteColors;

    private void Start()
    {
        InitializeColorSwapTexture();
        if (primaryColors != null)
        {
            AlterColorGroup(primaryColors, primaryHue, primarySaturation, primaryBrightness);
        }
        if (secondaryColors != null)
        {
            AlterColorGroup(secondaryColors, secondaryHue, secondarySaturation, secondaryBrightness);
        }
        if (accentColors != null)
        {
            AlterColorGroup(accentColors, accentHue, accentSaturation, accentBrightness);
        }
    }

    public void InitializeColorSwapTexture()
    {
        Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;

        for (int i = 0; i < colorSwapTex.width; ++i)
        {
            colorSwapTex.SetPixel(i, 0, new Color(0f, 0f, 0f, 0f));
        }
        colorSwapTex.Apply();

        this.gameObject.GetComponent<SpriteRenderer>().material.SetTexture("_SwapTex", colorSwapTex);

        spriteColors = new Color[colorSwapTex.width];
        colorPalette = colorSwapTex;
    }

    private void SwapColor(int startColor, Color32 newColor)
    {
        spriteColors[startColor] = newColor;
        colorPalette.SetPixel(startColor, 0, newColor);
        colorPalette.Apply();
    }

    private void AlterColorGroup(Color32[] colorGroup, float hueOffset, float saturationOffset, float brightnessOffset)
    {
        for (int i = 0; i < colorGroup.Length; i++)
        {
            HSB hsb = new HSB();
            RGB rgb = new RGB();

            rgb.r = colorGroup[i].r;
            rgb.g = colorGroup[i].g;
            rgb.b = colorGroup[i].b;

            hsb = HSBFromRGB(rgb.r, rgb.g, rgb.b);
            hsb.hue += hueOffset;
            hsb.saturation += saturationOffset;
            hsb.brightness += brightnessOffset;
            rgb = RGBFromHSB(hsb.hue, hsb.saturation, hsb.brightness);

            Color newColor = new Color(rgb.r / 255, rgb.g / 255, rgb.b / 255);
            float num = colorGroup[i].r;
            int startColor = (int)num;

            SwapColor(startColor, newColor);
        }
    }

    public RGB RGBFromHSB(float hue, float saturation, float brightness)
    {
        RGB color = new RGB();
        int i;
        float f, p, q, k;

        hue /= 60;
        i = (int)Mathf.Floor(hue);
        f = hue - i;
        p = brightness * (1 - saturation);
        q = brightness * (1 - saturation * f);
        k = brightness * (1 - saturation * (1 - f));
        switch (i)
        {
            case 0:
                color.r = brightness;
                color.g = k;
                color.b = p;
                break;
            case 1:
                color.r = q;
                color.g = brightness;
                color.b = p;
                break;
            case 2:
                color.r = p;
                color.g = brightness;
                color.b = k;
                break;
            case 3:
                color.r = p;
                color.g = q;
                color.b = brightness;
                break;
            case 4:
                color.r = k;
                color.g = p;
                color.b = brightness;
                break;
            default:
                color.r = brightness;
                color.g = p;
                color.b = q;
                break;
        }
        color.r *= 255;
        color.g *= 255;
        color.b *= 255;
        color.r = (int)color.r;
        color.g = (int)color.g;
        color.b = (int)color.b;
        return color;
    }

    public HSB HSBFromRGB(float r, float g, float b)
    {
        HSB color = new HSB();

        float min = Mathf.Min(r, g, b);
        float max = Mathf.Max(r, g, b);
        float delta = max - min;
        float hue = 0;
        float saturation = 0;
        float brightness = max / 255;

        if (max == r)
        {
            hue = (g - b) / delta;
        }
        if (max == g)
        {
            hue = 2 + (b - r) / delta;
        }
        if (max == b)
        {
            hue = 4 + (r - g) / delta;
        }
        hue *= 60;
        if (hue < 0)
        {
            hue += 360;
        }
        else if (hue > 360)
        {
            hue -= 360;
        }
        hue = (int)hue;
        if (max != 0)
        {
            saturation = delta / max;
        }
        else
        {
            saturation = 0;
            hue = 0;
        }
        color.hue = hue;
        color.saturation = saturation;
        color.brightness = brightness;
        return color;
    }

    public struct HSB
    {
        public float hue;
        public float saturation;
        public float brightness;
    }

    public struct RGB
    {
        //public byte r;
        public float r;
        public float g;
        public float b;
    }
}