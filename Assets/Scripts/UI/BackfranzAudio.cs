using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Backfranz.Playable;

public sealed class BackfranzAudio : MonoBehaviour
{
    internal const string ClipFolder = "Backfranz/Audio/";
    readonly Dictionary<string,AudioClip> clips = new Dictionary<string,AudioClip>();
    readonly System.Random variation = new System.Random();
    const int StepVariants = 5;
    AudioSource effects, footsteps, ambience, music, dialogueVoice;
    int stepVariant = -1;
    [SerializeField] AudioClip footstepClip, doorClip, liftClip, scannerClip, universityMusic, horrorMusic;
    public AudioMixerGroup AmbientGroup => ambience.outputAudioMixerGroup;

    void Awake()
    {
        var mixer = Resources.Load<AudioMixer>("Backfranz/MezclaBackfranz");
        effects = CreateSource(mixer,"Efectos");
        dialogueVoice = CreateSource(mixer,"Efectos");
        footsteps = CreateSource(mixer,"Pasos");
        ambience = CreateSource(mixer,"Ambiente");
        music = CreateSource(mixer,"Música");
        music.loop = true;
    }

    AudioSource CreateSource(AudioMixer mixer,string groupName)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.ignoreListenerPause = true;
        source.playOnAwake = false;
        if(mixer)
            source.outputAudioMixerGroup = Array.Find(mixer.FindMatchingGroups(groupName),group=>group.name==groupName);
        return source;
    }

    void Play(AudioSource output,string key,float volume,AudioClip replacement=null)
    {
        if(replacement)
        {
            output.PlayOneShot(replacement,volume);
            return;
        }
        var clip = Clip(key);
        if(clip) output.PlayOneShot(clip,volume);
    }

    AudioClip Clip(string key)
    {
        if(!clips.TryGetValue(key,out var clip))
        {
            clip = Resources.Load<AudioClip>(ClipFolder+key);
            clips.Add(key,clip);
            if(!clip) Debug.LogWarning("Falta el sonido de Backfranz: "+key,this);
        }
        return clip;
    }

    public void Click() => Play(effects,"ui-mover",.68f);
    public void Back() => Play(effects,"ui-volver",.67f);
    public void Confirm() => Play(effects,"ui-confirmar",.61f);
    public void Thunder() => Play(effects,"descenso-umbral",.8f);
    public void DistantKnock() => Play(ambience,"golpe-tras-muro",.65f);
    public void ShadowWarning() => Play(ambience,"sombra-alerta",.86f);
    public void ShadowCaught() => Play(effects,"sombra-retorno",.82f);
    public void Footstep() => Footstep(SurfaceSound.Baldosa);

    public void Footstep(SurfaceSound surface, bool running=false)
    {
        string material = surface switch
        {
            SurfaceSound.Adoquin => "adoquin",
            SurfaceSound.Madera => "madera",
            SurfaceSound.Metal => "metal",
            SurfaceSound.Alfombra => "alfombra",
            _ => "baldosa"
        };
        int next = variation.Next(stepVariant < 0 ? StepVariants : StepVariants-1);
        if(stepVariant >= 0 && next >= stepVariant) next++;
        stepVariant = next;
        footsteps.pitch = (running ? 1.04f : .97f)+(float)variation.NextDouble()*.08f;
        Play(footsteps,"paso-"+material+"-"+(stepVariant+1),running ? .78f : .61f,footstepClip);
    }

    public void Door() => Door(false);
    public void Door(bool heavy) => Play(effects,heavy?"puerta-metal":"puerta-vidrio",.72f,doorClip);
    public void Latch() => Play(effects,"cerrojo",.79f);
    public void Lift() => Play(effects,"ascensor-campana",.75f,liftClip);
    public void LiftMotor() => Play(ambience,"ascensor-motor",.60f);
    public void LiftDoors(bool opening) => Play(effects,opening?"ascensor-abre":"ascensor-cierra",.76f);
    public void WoodDoor() => Play(effects,"puerta-madera",.70f,doorClip);
    public void Scanner() => Play(effects,"lector-escaneo",.72f,scannerClip);
    public void ScanResult(bool granted) => Play(effects,granted?"lector-permitido":"lector-rechazado",.75f);
    public void ToggleSwitch() => Play(effects,"tablero-interruptor",.72f);
    public void Playback() => Play(effects,"archivo-reproduccion",.75f);
    public void AuditoriumSpeaker() => Play(ambience,"terror-interferencia",.48f);
    public void RecordedVoice() => Play(effects,"auditorio-grabadora",.67f);
    public void ServicesRelay(float proximity) => Play(ambience,"terror-interferencia",.56f*proximity);
    public void PickUp() => Play(effects,"objeto-recoger",.68f);
    public void MechanismReleased() => Play(effects,"mecanismo-liberado",.7f);
    public void Plate(bool correct) => Play(effects,correct?"placa-paso":"placa-interferencia",.75f);
    public void DialogueVoice(string speaker,char letter,int index)
    {
        bool yamil=speaker=="Yamil";
        var clip=Clip(yamil?"dialogo-yamil":"dialogo-persona");
        if(!clip)return;
        int voice=0;
        foreach(char c in speaker)voice=(voice*31+c)%19;
        float register=yamil?1f:.88f+voice*.015f;
        float inflection=((letter+index)%7-3)*.014f;
        dialogueVoice.clip=clip;
        dialogueVoice.pitch=register+inflection;
        dialogueVoice.volume=.58f;
        dialogueVoice.Play();
    }

    public void StopDialogueVoice()=>dialogueVoice.Stop();
    public void Journal(bool opening) => Play(effects,opening?"diario-abrir":"diario-cerrar",.60f);
    public void SaveConfirmation() => Play(effects,"partida-guardada",.65f);
    public void Gathered() => Play(effects,"rescate-reencuentro",.63f);

    public void SetTitleMusic() => SetMusic(Clip("musica-menu"),.49f);

    public void SetCampusMusic(bool dark) =>
        SetMusic(dark ? horrorMusic : universityMusic,.40f);

    public void EndScore(EscapeArchive.Ending ending)
    {
        SetMusic(null,.65f);
        string key = ending switch
        {
            EscapeArchive.Ending.Amanecer => "final-amanecer",
            EscapeArchive.Ending.Eco => "final-eco",
            EscapeArchive.Ending.Umbral => "final-umbral",
            _ => null
        };
        if(key != null) Play(music,key,.78f);
    }

    void SetMusic(AudioClip clip,float volume)
    {
        music.volume = volume;
        if(music.clip==clip) return;
        music.Stop();
        music.clip = clip;
        if(clip) music.Play();
    }
}
