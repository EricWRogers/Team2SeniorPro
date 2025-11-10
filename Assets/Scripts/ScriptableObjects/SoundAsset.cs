using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundAsset", menuName = "Scriptable Objects/SoundAsset")]
public class SoundAsset : ScriptableObject
{
    public List<AudioClip> audioClips = new List<AudioClip>();
    
}
