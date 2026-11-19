using UnityEngine;
using UnityEngine.UI;

// Keep Galilea's menu artwork available for the gallery and preserve the normal opening.
public sealed class BackfranzTransition : MonoBehaviour
{
    public RawImage darkBackground;
    public Image flash;
    public Text phaseLabel;
    public BackfranzAudio sound;
    [Min(0)] public float delay = 5f;
    void Start()
    {
        SetAlpha(darkBackground,0); SetAlpha(flash,0);
    }
    static void SetAlpha(Graphic g,float a) { var c=g.color;c.a=a;g.color=c; }
}
