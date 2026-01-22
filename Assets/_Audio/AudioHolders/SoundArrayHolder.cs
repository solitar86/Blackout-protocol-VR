using UnityEngine;

[CreateAssetMenu(fileName = "New SoundArrayHolder", menuName = "Audio Holders/New SoundArrayHolder")]
public class SoundArrayHolder : ScriptableObject
{
    public Sound[] SoundArray;

    [HideInInspector]
    public Sound LastPlayedSound;
}
