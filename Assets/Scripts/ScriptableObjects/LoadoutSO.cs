using UnityEngine;

[CreateAssetMenu(fileName = "LoadoutSO", menuName = "Scriptable Objects/LoadoutSO")]
public class LoadoutSO : ScriptableObject
{
    [field:SerializeField] public int Slot0 { get; private set; }
    [field:SerializeField] public int Slot1 { get; private set; }
    [field:SerializeField] public int Slot2 { get; private set; }
    [field:SerializeField] public int Slot3 { get; private set; }
    [field:SerializeField] public int Slot4 { get; private set; }
    [field:SerializeField] public int Slot5 { get; private set; }
    [field:SerializeField] public int Slot6 { get; private set; }
    [field:SerializeField] public int Slot7 { get; private set; }
    [field:SerializeField] public int Slot8 { get; private set; }
    [field:SerializeField] public int Slot9 { get; private set; }

    public void SetSlotValue(int slot, int value)
    {
        switch(slot)
        {
            case 0:
                Slot0 = value;
                break;
            case 1:
                Slot1 = value;
                break;
            case 2:
                Slot2 = value;
                break;
            case 3:
                Slot3 = value;
                break;
            case 4:
                Slot4 = value;
                break;
            case 5:
                Slot5 = value;
                break;
            case 6:
                Slot6 = value;
                break;
            case 7:
                Slot7 = value;
                break;
            case 8:
                Slot8 = value;
                break;
            case 9:
                Slot9 = value;
                break;
            default:
                break;
        }
    }

    public int GetSlotValue(int slot)
    {
        return slot switch
        {
            0 => Slot0,
            1 => Slot1,
            2 => Slot2,
            3 => Slot3,
            4 => Slot4,
            5 => Slot5,
            6 => Slot6,
            7 => Slot7,
            8 => Slot8,
            9 => Slot9,
            _ => 0,
        };
    }
}
