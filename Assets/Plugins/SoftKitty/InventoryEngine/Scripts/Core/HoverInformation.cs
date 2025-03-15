using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftKitty.InventoryEngine
{
    /// <summary>
    /// This module shows detailed information about an item.
    /// </summary>
    public class HoverInformation : MonoBehaviour
    {
        #region Variables
        private static HoverInformation instance;
        private static InventoryHolder CompareHolder;
        public RectTransform Rect;
        public CanvasGroup Group;
        public Text NameText;
        public Text TypeText;
        public Text QualityText;
        public Text DesText;
        public Text ItemNumber;
        public Text ItemUpgrade;
        public GameObject ItemFav;
        public RawImage ItemIcon;
        public RawImage GlowIcon;
        public Image ItemFrame;
        public Image ItemBg;
        public Text[] Stats;
        public GameObject EnchantLine;
        public Text[] Enchantments;
        public GameObject[] SocketSlots;
        public RawImage[] SocketIcons;
        public GameObject[] SocketLocks;
        public GameObject SocketLine;
        public GameObject[] Hints;
        private ItemIcon HoverSource;
        public Image CurrencyIcon;
        public Text CurrencyNumber;
        #endregion

        #region Internal Methods
        private void Start()
        {
            ItemFav.GetComponent<Image>().color= InventorySkin.instance.FavoriteColor;
        }

        private string GetHintText(ClickSetting _setting)
        {
            string _result = "";
            if (_setting.key != AlterKeys.None) _result += _setting.key.ToString().Replace("Left","")+"+";
            _result += _setting.mouseButton.ToString()+" to ";
            return _result;
        }

        public void _showHoverInfo(ItemIcon _source, Item _item, int _num, RectTransform _anchor, float _priceMultiplier,string _clickAction= "Use", bool _promoSplit = true, bool _promoFav = true, bool _promoDrop = true)
        {
            if (HoverSource == _source) return;

            Rect.position = _anchor.position;
            Group.alpha = 1F;
            Group.gameObject.SetActive(true);
            NameText.text = _item.nameWithAffixing + (_item.upgradeLevel > 0 ? " +" + _item.upgradeLevel.ToString() : "");
            TypeText.text = "[ " + _item.GetTypeName() + " ]";
            TypeText.color = Color.Lerp(_item.GetTypeColor(), Color.white, 0.4F);
            QualityText.text = _item.GetQualityName();
            QualityText.color = Color.Lerp(_item.GetQualityColor(), Color.white, 0.4F);
            ItemBg.color = _item.GetTypeColor();
            ItemFrame.color = _item.GetQualityColor();
            ItemIcon.texture = _item.icon;
            GlowIcon.texture = _item.icon;
            if (ItemManager.instance.EnableEnhancingGlow)
            {
                GlowIcon.gameObject.SetActive(_item.upgradeLevel > 0);
                GlowIcon.color = Color.Lerp(Color.black, new Color(1F, 0.6F, 0.04F, 1F), ItemManager.instance.EnhancingGlowCurve.Evaluate(Mathf.Clamp01(_item.upgradeLevel * 1F / ItemManager.instance.MaxiumEnhancingLevel)));
            }
            else
            {
                GlowIcon.gameObject.SetActive(false);
            }
            ItemNumber.text = _num > 0 ? _num.ToString() : "";
            ItemFav.SetActive(_item.favorite);
            DesText.text = _item.description;
            foreach (var obj in Hints) {
                obj.SetActive(false);
            }
            int _hintIndex = 0;
            foreach (var setting in ItemManager.instance.clickSettings)
            {
                if (_hintIndex < 4) {
                    switch (setting.function) {
                        case ClickFunctions.Use:
                            if ((_item.useable || _clickAction != "Use") && _clickAction.Length > 0) {
                                Hints[_hintIndex].GetComponent<Text>().text = GetHintText(setting) + _clickAction;
                                Hints[_hintIndex].SetActive(true);
                                _hintIndex++;
                            }
                            break;
                        case ClickFunctions.Split:
                            if (_item.maxiumStack > 1 && _num > 1 && _promoSplit)
                            {
                                Hints[_hintIndex].GetComponent<Text>().text = GetHintText(setting) + "Split";
                                Hints[_hintIndex].SetActive(true);
                                _hintIndex++;
                            }
                            break;
                        case ClickFunctions.MarkFavorite:
                            if (_promoFav)
                            {
                                Hints[_hintIndex].GetComponent<Text>().text = GetHintText(setting) + "Add Favorite";
                                Hints[_hintIndex].SetActive(true);
                                _hintIndex++;
                            }
                            break;
                        case ClickFunctions.Drop:
                            if (_item.deletable  && _promoDrop)
                            {
                                Hints[_hintIndex].GetComponent<Text>().text = GetHintText(setting) + "Drop";
                                Hints[_hintIndex].SetActive(true);
                                _hintIndex++;
                            }
                            break;
                    }
                }
            }

            CurrencyIcon.gameObject.SetActive(_item.tradeable);
            CurrencyIcon.sprite = ItemManager.instance.currencies[_item.currency].icon;
            CurrencyNumber.color = ItemManager.instance.currencies[_item.currency].color;
            CurrencyNumber.text = Mathf.CeilToInt(_item.price * _priceMultiplier).ToString();

            Dictionary<Attribute, float> _attDic = new Dictionary<Attribute, float>();
            foreach (Attribute obj in ItemManager.instance.itemAttributes) {
                if (obj.isNumber() && _item.GetAttributeFloat(obj.key) != 0F && obj.visible )
                {
                    _attDic.Add(obj.Copy(), _item.GetAttributeFloat(obj.key));
                }
            }
            List<Attribute> keyList = new List<Attribute>(_attDic.Keys);
            for (int i = 0; i < Stats.Length; i++)
            {
                if (i < keyList.Count)
                {
                    Stats[i].text = "<color=#" + ColorUtility.ToHtmlStringRGB(ItemManager.instance.AttributeNameColor) + ">" + keyList[i].name + "</color> : ";
                    float _value = _attDic[keyList[i]];
                    Stats[i].text += (_value > 0F ? "+" : "") + _value.ToString("0.0");
                    if (CompareHolder != null)
                    {
                        float _compareValue = _value - CompareHolder.GetAttributeValueByTag(keyList[i].key, _item.tags);
                        if (_compareValue != 0F)
                            Stats[i].text += (_compareValue > 0F ? "<color=#469824>" : "<color=#BF3126>") + "(" + (_compareValue > 0F ? "+" : "") + _compareValue.ToString("0.0") + ")</color>";
                    }
                    Stats[i].gameObject.SetActive(true);
                }
                else
                {
                    Stats[i].gameObject.SetActive(false);
                }
            }
            EnchantLine.SetActive(_item.enchantments.Count > 0 && ItemManager.instance.EnableEnchanting);
            for (int i = 0; i < Enchantments.Length; i++)
            {
                if (i < _item.enchantments.Count && ItemManager.instance.EnableEnchanting)
                {
                    Enchantments[i].text = ItemManager.enchantmentDic[_item.enchantments[i]].GetDescription();
                    Enchantments[i].gameObject.SetActive(true);
                }
                else
                {
                    Enchantments[i].gameObject.SetActive(false);
                }
            }
            SocketLine.SetActive(_item.socketingSlots>0 && ItemManager.instance.EnableSocketing);
            for (int i = 0; i < SocketSlots.Length; i++)
            {
                if (i < _item.socketedItems.Count && ItemManager.instance.EnableSocketing)
                {
                    SocketLocks[i].SetActive (_item.socketedItems[i]==-2);
                    SocketIcons[i].gameObject.SetActive(_item.socketedItems[i]>=0);
                    if (_item.socketedItems[i] >= 0) SocketIcons[i].texture = ItemManager.itemDic[_item.socketedItems[i]].icon;
                    SocketSlots[i].SetActive(true);
                }
                else
                {
                    SocketSlots[i].SetActive(false);
                }
            }
            HoverSource = _source;
        }


        void Update()
        {
            if (HoverSource != null)
            {
                if (HoverSource.GetHover())
                {
                    Group.alpha = 1F;
                }
                else
                {
                    FadeOff();
                }
            }
            else
            {
                FadeOff();
            }
        }

        void FadeOff()
        {
            if (Group.alpha > 0F)
            {
                Group.alpha = Mathf.MoveTowards(Group.alpha, 0F, Time.deltaTime * 4F);
            }
            else if (Group.gameObject.activeSelf)
            {
                Group.gameObject.SetActive(false);
                HoverSource = null;
            }
        }
        #endregion


        /// <summary>
        /// Sets an InventoryHolder so that the hovering item's attributes can be compared with the equipment of this InventoryHolder.
        /// </summary>
        /// <param name="_holder"></param>
        public static void SetCompareHolder(InventoryHolder _holder) 
        {
            CompareHolder = _holder;
        }

        /// <summary>
        /// Shows detailed information for the provided item icon. The information panel will align with the _anchor RectTransform.
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="_item"></param>
        /// <param name="_num"></param>
        /// <param name="_anchor"></param>
        /// <param name="_priceMultiplier"></param>
        /// <param name="_clickAction"></param>
        /// <param name="_promoSplit"></param>
        /// <param name="_promoFav"></param>
        public static void ShowHoverInfo(ItemIcon _source, Item _item, int _num ,RectTransform  _anchor, float _priceMultiplier, string _clickAction="Use", bool _promoSplit=true, bool _promoFav=true, bool _promoDrop=true)
        {
            if (instance == null)
            {
                GameObject newObj = Instantiate(Resources.Load<GameObject>("InventoryEngine/HoverInformation"), _anchor.GetComponentInParent<CanvasScaler>().transform);
                newObj.transform.SetAsLastSibling();
                newObj.transform.localPosition = Vector3.zero;
                newObj.transform.localScale = Vector3.one;
                instance = newObj.GetComponent<HoverInformation>();
            }
            instance._showHoverInfo(_source,_item, _num, _anchor, _priceMultiplier, _clickAction, _promoSplit, _promoFav, _promoDrop);
        }

        
    }
}
