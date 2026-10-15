using UnityEngine;

// Cristopher: procedural original UI sounds, no external audio dependencies.
public sealed class BackfranzAudio : MonoBehaviour
{
    private AudioSource effects, ambience;
    private AudioClip click, thunder, hum;
    void Awake()
    {
        effects = gameObject.AddComponent<AudioSource>();
        ambience = gameObject.AddComponent<AudioSource>();
        effects.ignoreListenerPause = ambience.ignoreListenerPause = true;
        effects.playOnAwake = ambience.playOnAwake = false;
        click = Make("Seleccion", .09f, false);
        thunder = Make("Trueno", 2.4f, true);
        int rate = 22050;
        float[] data = new float[rate * 2];
        for (int i=0;i<data.Length;i++) { float t=i/(float)rate; data[i]=.06f*Mathf.Sin(2*Mathf.PI*60*t)+.025f*Mathf.Sin(2*Mathf.PI*120*t); }
        hum=AudioClip.Create("Fluorescentes",data.Length,1,rate,false); hum.SetData(data,0);
        ambience.clip=hum; ambience.loop=true;
    }
    AudioClip Make(string name, float duration, bool noise)
    {
        const int rate=22050;
        float[] data=new float[(int)(rate*duration)];
        var random=new System.Random(64); float low=0;
        for(int i=0;i<data.Length;i++) {
            float t=i/(float)rate, envelope=Mathf.Min(1,t/.01f)*Mathf.Pow(1-t/duration,2);
            low=Mathf.Lerp(low,(float)random.NextDouble()*2-1,.07f);
            data[i]=noise ? envelope*(low*.8f+Mathf.Sin(t*190)*.1f) : envelope*Mathf.Sin(2*Mathf.PI*660*t)*.12f;
        }
        var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
    }
    public void Click() { effects.PlayOneShot(click); }
    public void Thunder() { effects.PlayOneShot(thunder); ambience.Play(); }
    void OnDestroy() { Destroy(click); Destroy(thunder); Destroy(hum); }
}
