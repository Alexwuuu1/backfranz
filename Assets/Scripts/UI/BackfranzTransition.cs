using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Galilea: one transition per menu entry, using unscaled time.
public sealed class BackfranzTransition : MonoBehaviour
{
    public RawImage darkBackground;
    public Image flash;
    public Text phaseLabel;
    public BackfranzAudio sound;
    [Min(0)] public float delay = 5f;
    public bool Completed { get; private set; }
    IEnumerator Start()
    {
        SetAlpha(darkBackground,0); SetAlpha(flash,0);
        yield return new WaitForSecondsRealtime(delay);
        sound.Thunder();
        bool reduced=PlayerPrefs.GetInt("Backfranz.ReducedFlashes",0)==1;
        if(!reduced) {
            SetAlpha(flash,.8f); yield return new WaitForSecondsRealtime(.07f);
            SetAlpha(flash,0); SetAlpha(darkBackground,.65f);
            yield return new WaitForSecondsRealtime(.16f);
            SetAlpha(flash,.55f);
        }
        float elapsed=0;
        while(elapsed<.65f) {
            elapsed+=Time.unscaledDeltaTime;
            SetAlpha(darkBackground,Mathf.Clamp01(elapsed/.65f));
            SetAlpha(flash,reduced?0:Mathf.Lerp(.55f,0,elapsed/.65f));
            yield return null;
        }
        SetAlpha(darkBackground,1); SetAlpha(flash,0); Completed=true;
        phaseLabel.text="EL SEMESTRE NO TERMINA AQUÍ";
    }
    static void SetAlpha(Graphic g,float a) { var c=g.color;c.a=a;g.color=c; }
}
