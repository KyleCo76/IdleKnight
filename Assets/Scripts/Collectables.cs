using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    [FoldoutGroup("Collectable Settings")]
    [FoldoutGroup("Collectable Settings/General Settings"), SerializeField, Tooltip("Should this object be collected on pickup?")]
    private bool collectOnPickup = true;
    [FoldoutGroup("Collectable Settings/General Settings"), SerializeField, Tooltip("Should this object be destroyed on pickup?")]
    private bool destroyOnPickup = true;
    [FoldoutGroup("Collectable Settings/Audio Settings"), SerializeField, Tooltip("Sound to play on pickup")]
    private AudioClip pickupSound;
    [FoldoutGroup("Collectable Settings/Audio Settings"), SerializeField, Tooltip("Volume of the pickup sound"), Range(0f, 1f)]
    private float pickupSoundVolume = 1f;
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("Should the sprite be randomized?")]
    private bool randomizeSprite = false;
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("List of sprites? If false, will default to a folder selection"), ShowIf("randomizeSprite")]
    private bool useCustomSpriteList = false;
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("List of sprites to choose from"), ShowIf("useCustomSpriteList")]
    private List<Sprite> customSpriteList = new List<Sprite>();
    [FoldoutGroup("Collectable Settings/Sprite Settings"), SerializeField, Tooltip("Folder to load sprites from"), FolderPath, ShowIf("@this.randomizeSprite && !this.useCustomSpriteList")]
    private string spriteFolderPath = "";
    [FoldoutGroup("Collectable Settings/Collection Settings"), SerializeField, Tooltip("Should the stats be randomized?")]
    private bool randomizeStats = false;

    [FoldoutGroup("Collectable Stats")]
    [FoldoutGroup("Collectable Stats/Static Stats"), SerializeField, Tooltip("Health to give on pickup"), ShowIf("@!this.randomizeStats")]
    private float healthAmount = 0f;



    private void OnValidate()
    {
        if (!randomizeSprite) {
            useCustomSpriteList = false;
        }
    }

}
