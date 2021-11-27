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
                
            textMesh.transform.position = Camera.main.WorldToScreenPoint(character.transform.position + new Vector3(0 , 1.5f , 0));
            if(Vector3.Dot((character.transform.position - Camera.main.transform.position) ,  Camera.main.transform.forward) < 0)
                textMesh.gameObject.SetActive(false);
            else
                textMesh.gameObject.SetActive(true);
        }
    }
    
    public void OnCharacterJoin(FpsCharacter fpsCharacter)
    {
        if(dictCharToNameText.ContainsKey(fpsCharacter))    return;
        
        GameObject nameOverlayObj = Instantiate(nameOverlayPrefab , LocalContext.Instance.dynamicCanvas.transform);
        TextMeshProUGUI textMesh = nameOverlayObj.GetComponent<TextMeshProUGUI>();
        textMesh.SetText(fpsCharacter.characterName);
        
        dictCharToNameText.Add(fpsCharacter , textMesh);
    }
        
    public void SyncAllWithContext()
    {
        foreach(FpsCharacter fpsCharacter in SharedContext.Instance.characterList)
        {
            OnCharacterJoin(fpsCharacter);
        }
    }
}
