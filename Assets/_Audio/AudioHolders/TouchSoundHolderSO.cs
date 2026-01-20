using UnityEngine;

[CreateAssetMenu(fileName = "New SoundHolder", menuName = "Audio Holders/New Touch SoundHolder")]
public class TouchSoundHolderSO : ScriptableObject
{
    public Sound FirstTouchSound;
    public Sound SlideTouchSound;
    public Sound EndTouchSound;
}
