using UnityEngine;

[CreateAssetMenu(fileName = "NewGamePlusSO", menuName = "Scriptable Objects/NewGamePlusSO")]
public class NewGamePlusSO : ScriptableObject
{
    [field:SerializeField] public bool IsNewGamePlus {get; private set; } = false;

    public void SetNewGamePlus()
    {
        IsNewGamePlus = true;
    }
}
