using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
* @Arthur Lacour
* @UIManager.cs
* @15/03/2018
* @Description:
*   - Actualise toute l'UI sur les différentes zones.
*       (En fonction de l'entitée sélectionner par le RaycastSelecter.cs.)
* 
* @Condition:
*   - S'accroche sur le parent du Canvas de la Scène.
*   - Remplir tous les champs public.
*/

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    struct PreviewZone
    {
        public Camera camera;
        public RawImage miniFace;
    }
    [System.Serializable]
    struct InfosZone
    {
        public TextMeshProUGUI nameField;
        public Slider durabilitySlider;
        public TextMeshProUGUI durabilityText;
        public Slider hungerSlider;
        public TextMeshProUGUI description;
    }
    [System.Serializable]
    struct ContaintZone
    {
        public TextMeshProUGUI[] slots;
        public GameObject buttonsParent;
        public Button[] buttons;
        public Slider slider;
    }
    [SerializeField] PreviewZone previewZone;
    [SerializeField] InfosZone infoZone;
    [SerializeField] ContaintZone containtZone;

    private int currentDurability;
    private float currentHunger;
    private int numNullContaint;
    private int[] tabContaints;

    Transform targetRef = null;
    Labourer peasantRef = null;
    BackpackComponent backpackRef = null;
    Block blockRef = null;
    SupplyPileComponent supplyPileRef = null;
    InnComponent innRef = null;
    SchoolComponent schoolRef = null;
    MetallurgyComponent metalRef = null;
    Projector projRef = null;

    public static UIManager Instance;

    #region Getteurs
    public MetallurgyComponent MetalRef
    {
        get { return metalRef; }
    }
    #endregion

    private void Start()
    {
        if (Instance != null)
            Destroy(this);
        else
            Instance = this;

        EnableAllInfos(false);
        EnableAllContaints(false);
    }

    private void FixedUpdate()
    {
        if (targetRef == null)
            return;

        UpdateInfos(targetRef);
        UpdateContaints(targetRef);
    }

    public void SelectEntity(bool _activate, Transform _target)
    {
        targetRef = _target;

        ManagePreviewCamera(_activate, _target);
        ImportInfos(_activate, _target);
        ImportContaints(_activate, _target);
        ActiveFeedback(_activate, _target);
    }

    private void ManagePreviewCamera(bool _activate, Transform _camTarget)
    {
        if (_camTarget != null)
        {
            previewZone.camera.transform.position = _camTarget.position + _camTarget.forward * 2.5f + Vector3.up;
            previewZone.camera.transform.LookAt(_camTarget.position + Vector3.up);
        }
        previewZone.camera.transform.SetParent(_camTarget);

        previewZone.camera.enabled = _activate;
        previewZone.miniFace.enabled = _activate;
    }

    private void ImportInfos(bool _activate, Transform _target)
    {
        EnableAllInfos(_activate);

        if (_activate)
        {
            infoZone.nameField.text = _target.name;

            if (_target.tag == "Peasant")
            {
                peasantRef = _target.GetComponent<Labourer>();

                infoZone.durabilitySlider.value = (float)peasantRef.currentLife / (float)peasantRef.maxLife;
                infoZone.durabilityText.text = peasantRef.currentLife + " / " + peasantRef.maxLife;
                currentDurability = peasantRef.currentLife;
                infoZone.hungerSlider.value = (float)peasantRef.currentHunger / (float)peasantRef.maxHunger;
                currentHunger = peasantRef.currentHunger;
                infoZone.description.text = peasantRef.description;
            }
            else if (_target.tag == "Block")
            {
                blockRef = _target.GetComponent<Block>();

                infoZone.durabilitySlider.value = (float)blockRef.currentLife / (float)blockRef.maxLife;
                infoZone.durabilityText.text = blockRef.currentLife + " / " + blockRef.maxLife;
                currentDurability = blockRef.currentLife;
                infoZone.hungerSlider.gameObject.SetActive(false);
                infoZone.description.text = blockRef.description;
            }
        }
    }
    private void ImportContaints(bool _activate, Transform _target)
    {
        EnableAllContaints(_activate);

        if (_activate)
        {
            //Cleans
            tabContaints = new int[containtZone.slots.Length];
            for (int i = 0; i < containtZone.slots.Length; i++)
                containtZone.slots[i].text = "";
            EnableContaintButtons(false);
            EnableContaintSlider(false);

            if (_target.tag == "Peasant")
            {
                backpackRef = _target.GetComponent<BackpackComponent>();
                UpdateAllContaintStrings(backpackRef);
            }
            else if (_target.tag == "Block")
            {
                if (_target.GetComponent<SupplyPileComponent>() != null)
                {
                    UpdateRef(_target.GetComponent<SupplyPileComponent>());
                    UpdateAllContaintStrings(supplyPileRef);
                }
                else if (_target.GetComponent<InnComponent>() != null)
                {
                    UpdateRef(_target.GetComponent<InnComponent>());
                    UpdateAllContaintStrings(innRef);
                }
                else if (_target.GetComponent<SchoolComponent>() != null)
                {
                    UpdateRef(_target.GetComponent<SchoolComponent>());
                    EnableContaintSlots(false);
                    EnableContaintButtons(true);
                    UpdateAllContaintStrings(schoolRef);
                }
                else if (_target.GetComponent<MetallurgyComponent>() != null)
                {
                    UpdateRef(_target.GetComponent<MetallurgyComponent>());
                    EnableContaintSlider(true);
                    UpdateAllContaintStrings(metalRef);
                }
                else {
                    UpdateRef();
                }
            }
        }
    }
    private void ActiveFeedback(bool _activate, Transform _target)
    {
        if (projRef != null) {
            projRef.enabled = false;
            projRef = null;
        }
        if (_activate)
        {
            if (_target.GetComponent<DecalProj>() != null)
            {
                projRef = _target.GetComponent<DecalProj>().Projector;
                projRef.enabled = true;
            }
        }
    }
    private void UpdateInfos(Transform _target)
    {
        if (_target.tag == "Peasant")
        {
            if (peasantRef.currentLife != currentDurability || peasantRef.currentHunger != currentHunger)
            {
                infoZone.durabilitySlider.value = (float)peasantRef.currentLife / (float)peasantRef.maxLife;
                infoZone.durabilityText.text = peasantRef.currentLife + " / " + peasantRef.maxLife;
                currentDurability = peasantRef.currentLife;
                infoZone.hungerSlider.value = (float)peasantRef.currentHunger / (float)peasantRef.maxHunger;
                currentHunger = peasantRef.currentHunger;
            }
        }
        else if (_target.tag == "Block")
        {
            if (blockRef.currentLife != currentDurability)
            {
                infoZone.durabilitySlider.value = (float)blockRef.currentLife / (float)blockRef.maxLife;
                infoZone.durabilityText.text = blockRef.currentLife + " / " + blockRef.maxLife;
                currentDurability = blockRef.currentLife;
            }
        }
    }
    private void UpdateContaints(Transform _target)
    {
        if (_target.tag == "Peasant")
        {
            if (AsAnyOneContaintChange(backpackRef))
                UpdateAllContaintStrings(backpackRef);
        }
        else if (_target.tag == "Block")
        {
            if (supplyPileRef != null)
            {
                if(AsAnyOneContaintChange(supplyPileRef))
                    UpdateAllContaintStrings(supplyPileRef);
            }
            else if (innRef != null)
            {
                if (AsAnyOneContaintChange(innRef))
                    UpdateAllContaintStrings(innRef);
            }
            else if (metalRef != null)
            {
                if (AsAnyOneContaintChange(metalRef))
                    UpdateAllContaintStrings(metalRef);
            }
        }
    }

    private void UpdateRef()
    {
        supplyPileRef = null;
        innRef = null;
        schoolRef = null;
        metalRef = null;
    }
    private void UpdateRef(SupplyPileComponent _supplyPile)
    {
        UpdateRef();
        supplyPileRef = _supplyPile;
    }
    private void UpdateRef(InnComponent _inn)
    {
        UpdateRef();
        innRef = _inn;
    }
    private void UpdateRef(SchoolComponent _school)
    {
        UpdateRef();
        schoolRef = _school;
    }
    private void UpdateRef(MetallurgyComponent _metal)
    {
        UpdateRef();
        metalRef = _metal;
    }

    private bool AsAnyOneContaintChange(BackpackComponent _backpack)
    {
        bool asChange = false;
        int currentNumNullContaint = 0;
        for (int i = containtZone.slots.Length - 1; i >= 0; i--)
        {
            asChange = true;
            for (int idSlot = 0; idSlot < containtZone.slots.Length; idSlot++)
            {
                if (GetNextInfo(_backpack, i) == containtZone.slots[idSlot].text) {
                    asChange = false;
                    break;
                }
            }
            if (asChange == true)
                return true;
            if (GetNextInfo(_backpack, i) == "")
                currentNumNullContaint++;
        }

        if (numNullContaint != currentNumNullContaint)
            return true;

        return false;
    }
    private bool AsAnyOneContaintChange(SupplyPileComponent _supplyPile)
    {
        bool asChange = false;
        int currentNumNullContaint = 0;
        for (int i = containtZone.slots.Length - 1; i >= 0; i--)
        {
            asChange = true;
            for (int idSlot = 0; idSlot < containtZone.slots.Length; idSlot++)
            {
                if (GetNextInfo(_supplyPile, i) == containtZone.slots[idSlot].text)
                {
                    asChange = false;
                    break;
                }
            }
            if (asChange == true)
                return true;
            if (GetNextInfo(_supplyPile, i) == "")
                currentNumNullContaint++;
        }

        if (numNullContaint != currentNumNullContaint)
            return true;

        return false;
    }
    private bool AsAnyOneContaintChange(InnComponent _inn)
    {
        bool asChange = false;
        int currentNumNullContaint = 0;
        for (int i = containtZone.slots.Length - 1; i >= 0; i--)
        {
            asChange = true;
            for (int idSlot = 0; idSlot < containtZone.slots.Length; idSlot++)
            {
                if (GetNextInfo(_inn, i) == containtZone.slots[idSlot].text)
                {
                    asChange = false;
                    break;
                }
            }
            if (asChange == true)
                return true;
            if (GetNextInfo(_inn, i) == "")
                currentNumNullContaint++;
        }

        if (numNullContaint != currentNumNullContaint)
            return true;

        return false;
    }
    private bool AsAnyOneContaintChange(MetallurgyComponent _metal)
    {
        return "<color=yellow>GoldChest</color> : " + _metal.numGoldChest != containtZone.slots[0].text;
    }

    private void UpdateAllContaintStrings(BackpackComponent _backpack)
    {
        int idSlot = 0;
        string backbackString = "";

        numNullContaint = containtZone.slots.Length;
        for (int i = containtZone.slots.Length - 1; i >= 0; i--)
        {
            backbackString = GetNextInfo(_backpack, i);

            containtZone.slots[idSlot].text = backbackString;
            if (backbackString != "") {
                numNullContaint--;
                idSlot++;
            }
        }
    }
    private void UpdateAllContaintStrings(SupplyPileComponent _supplyPile)
    {
        int idSlot = 0;
        string supplyString = "";

        numNullContaint = containtZone.slots.Length;
        for (int i = containtZone.slots.Length - 1; i >= 0; i--)
        {
            supplyString = GetNextInfo(_supplyPile, i);

            containtZone.slots[idSlot].text = supplyString;
            if (supplyString != "") {
                numNullContaint--;
                idSlot++;
            }
        }
    }
    private void UpdateAllContaintStrings(InnComponent _inn)
    {
        int idSlot = 0;
        string innString = "";

        numNullContaint = containtZone.slots.Length;
        for (int i = containtZone.slots.Length - 1; i >= 0; i--)
        {
            innString = GetNextInfo(_inn, i);

            containtZone.slots[idSlot].text = innString;
            if (innString != "") {
                numNullContaint--;
                idSlot++;
            }
        }
    }
    private void UpdateAllContaintStrings(SchoolComponent _school)
    {
        for (int i = 0; i < containtZone.slots.Length; i++) {
            containtZone.slots[i].text = "";
        }
    }
    private void UpdateAllContaintStrings(MetallurgyComponent _metal)
    {
        for (int i = 1; i < containtZone.slots.Length; i++)
            containtZone.slots[i].enabled = false;
        containtZone.slots[0].text = "<color=yellow>GoldChest</color> : " + _metal.numGoldChest;
        containtZone.slider.value = _metal.numOrder;
    }

    private string GetNextInfo(BackpackComponent _backpack, int _idInfo)
    {
        string returnString = "";

        switch (_idInfo)
        {
            case 0:
                tabContaints[_idInfo] = _backpack.numLogs;
                returnString = _backpack.numLogs > 0 ? "<color=#8B4513>Logs</color> : " + _backpack.numLogs : "";
                break;
            case 1:
                tabContaints[_idInfo] = _backpack.numFirewood;
                returnString = _backpack.numFirewood > 0 ? "<color=#8B4513>Firewood</color> : " + _backpack.numFirewood : "";
                break;
            case 2:
                tabContaints[_idInfo] = _backpack.numOre;
                returnString = _backpack.numOre > 0 ? "<color=yellow>Ore</color> : " + _backpack.numOre : "";
                break;
            case 3:
                tabContaints[_idInfo] = _backpack.numWheat;
                returnString = _backpack.numWheat > 0 ? "<color=green>Wheat</color> : " + _backpack.numWheat : "";
                break;
            case 4:
                tabContaints[_idInfo] = _backpack.numBreads;
                returnString = _backpack.numBreads > 0 ? "<color=blue>Bread</color> : " + _backpack.numBreads : "";
                break;
            default:
                returnString = "";
                break;
        }

        return returnString;
    }
    private string GetNextInfo(SupplyPileComponent _supplyPile, int _idInfo)
    {
        string returnString = "";

        switch (_idInfo)
        {
            case 0:
                tabContaints[_idInfo] = _supplyPile.numLogs;
                returnString = _supplyPile.numLogs > 0 ? "<color=#8B4513>Logs</color> : " + _supplyPile.numLogs : "";
                break;
            case 1:
                tabContaints[_idInfo] = _supplyPile.numFirewood;
                returnString = _supplyPile.numFirewood > 0 ? "<color=#8B4513>Firewood</color> : " + _supplyPile.numFirewood : "";
                break;
            case 2:
                tabContaints[_idInfo] = _supplyPile.numOre;
                returnString = _supplyPile.numOre > 0 ? "<color=yellow>Ore</color> : " + _supplyPile.numOre : "";
                break;
            case 3:
                tabContaints[_idInfo] = _supplyPile.numTools;
                returnString = _supplyPile.numTools > 0 ? "<color=red>Tools</color> : " + _supplyPile.numTools : "";
                break;
            default:
                returnString = "";
                break;
        }

        return returnString;
    }
    private string GetNextInfo(InnComponent _inn, int _idInfo)
    {
        string returnString = "";

        switch (_idInfo)
        {
            case 0:
                tabContaints[_idInfo] = _inn.numWheat;
                returnString = _inn.numWheat > 0 ? "<color=green>Wheat</color> : " + _inn.numWheat : "";
                break;
            case 1:
                tabContaints[_idInfo] = _inn.numBreads;
                returnString = _inn.numBreads > 0 ? "<color=blue>Bread</color> : " + _inn.numBreads : "";
                break;
            default:
                returnString = "";
                break;
        }

        return returnString;
    }

    private void EnableAllInfos(bool _activate)
    {
        infoZone.nameField.enabled = _activate;
        infoZone.durabilitySlider.gameObject.SetActive(_activate);
        infoZone.durabilityText.enabled = _activate;
        infoZone.description.enabled = _activate;
        infoZone.hungerSlider.gameObject.SetActive(_activate);
    }
    private void EnableAllContaints(bool _activate)
    {
        EnableContaintSlots(_activate);
        EnableContaintButtons(_activate);
        EnableContaintSlider(_activate);
    }
    private void EnableContaintSlots(bool _activate)
    {
        for (int i = 0; i < containtZone.slots.Length; i++)
            containtZone.slots[i].enabled = _activate;
    }
    private void EnableContaintButtons(bool _activate)
    {
        containtZone.buttonsParent.SetActive(_activate);
    }
    private void EnableContaintSlider(bool _activate)
    {
        containtZone.slider.gameObject.SetActive(_activate);
    }


    public void School_OrderLabourer(int _idLabourer)
    {
        schoolRef.hasOrderLabourer = true;
        schoolRef.idLabourerOrder = _idLabourer;

        for (int i = 0; i < containtZone.buttons.Length; i++)
            if (i != _idLabourer)
                containtZone.buttons[i].interactable = false;
    }
}
