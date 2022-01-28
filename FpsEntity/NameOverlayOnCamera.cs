using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameOverlayOnCamera: MonoBehaviour
{
    public GameObject nameOverlayPrefab;
    private Dictionary<FpsCharacter, TextMeshProUGUI> dictCharToNameText = new Dictionary<FpsCharacter, TextMeshProUGUI>();
    public FpsPlayer ownerPlayer;
        
    void Start()
    {
        ownerPlayer = GetComponentInParent<FpsPlayer>();
        SharedContext.Instance.characterJoinEvent.AddListener(OnCharacterJoin);
        SharedContext.Instance.characterRemoveEvent.AddListener(OnCharacterRemove);
        SyncAllWithContext();
    }
    
    void Update()
    {
        if(ownerPlayer == null) return;
        UpdateNameOverlayPosition();
    }
    
    void UpdateNameOverlayPosition()
    {
        foreach(KeyValuePair<FpsCharacter, TextMeshProUGUI> entry in dictCharToNameText)
        {
            FpsCharacter character = entry.Key;
            TextMeshProUGUI textMesh = entry.Value;
            
            if(character.team != ownerPlayer.team || character.IsDead() || character == ownerPlayer)
            {
                textMesh.gameObject.SetActive(false);
                continue;
            }
                
            textMesh.transform.position = Camera.main.WorldToScreenPoint(character.transform.position + new Vector3(0 , 1.8f , 0));
            if(Vector3.Dot((character.transform.position - Camera.main.transform.position) ,  Camera.main.transform.forward) < 0)
                textMesh.gameObject.SetActive(false);
            else
            {
                textMesh.text = character.characterName;
                textMesh.gameObject.SetActive(true);
            }
        }
    }
    
    public void OnCharacterJoin(FpsCharacter fpsCharacter)
    {
        if(dictCharToNameText.ContainsKey(fpsCharacter))    return;
        
        GameObject nameOverlayObj = Instantiate(nameOverlayPrefab , LocalPlayerContext.Instance.inGameDynamicCanvas.transform);
        TextMeshProUGUI textMesh = nameOverlayObj.GetComponent<TextMeshProUGUI>();
        textMesh.SetText(fpsCharacter.characterName);
        
        dictCharToNameText.Add(fpsCharacter , textMesh);
    }
    
    public void OnCharacterRemove(FpsCharacter fpsCharacter)
    {
        if(dictCharToNameText.ContainsKey(fpsCharacter))
        {
            Destroy(dictCharToNameText[fpsCharacter]);
            dictCharToNameText.Remove(fpsCharacter);
        }
    }
        
    public void SyncAllWithContext()
    {
        foreach(FpsCharacter fpsCharacter in SharedContext.Instance.characterList)
        {
            OnCharacterJoin(fpsCharacter);
        }
    }
}
