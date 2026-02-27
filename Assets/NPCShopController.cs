using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCShopController : MonoBehaviour
{
    [Header("UI Settings")]
    public Text notifyText;
    public float notifyDuration = 2.0f; 
    private string selectedItemName = ""; 

    public GameObject[] highlightFrames; 
    public TextMeshProUGUI goldDisplayText; 
    public GameObject shopPanel;
    private bool isNearNPC = false; 
    private int selectedPrice = 0; 
    public GameObject goldUI;
    void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && isNearNPC)
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        if (shopPanel == null) return;
        bool isActive = !shopPanel.activeSelf;

        shopPanel.SetActive(isActive); 

       
        if (goldUI != null)
        {
            goldUI.SetActive(isActive);
        }

       
        if (isActive)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
           
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                HeroKnight playerScript = playerObj.GetComponent<HeroKnight>();
                UpdateGoldDisplay(playerScript.m_gold);
            }
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
        }

        if (!isActive)
        {
            selectedPrice = 0;
            foreach (GameObject glow in selectionGlows)
            {
                if (glow != null) glow.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isNearNPC = true;
            Debug.Log("Đã chạm vào vùng NPC ở khoảng cách: " + Vector2.Distance(transform.position, other.transform.position));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isNearNPC = false;
            Debug.Log("Đã rời xa NPC!");
        }
    }

    public void BuyItem(string itemName)
    {
        int price = 0;

        if (itemName == "Mau") price = 10;
        else if (itemName == "Stamina") price = 5;
        else if (itemName == "PhiTieu") price = 8;

        HeroKnight player = FindFirstObjectByType<HeroKnight>();
        if (player != null && player.SpendGold(price))
        {
            ShowNotify("Đã mua " + itemName + " thành công!");
        }
        else
        {
            ShowNotify("Không đủ vàng để mua " + itemName + "!");
        }
    }

    public void ShowNotify(string message)
    {
        StopAllCoroutines(); 
        StartCoroutine(NotifyRoutine(message));
    }

    IEnumerator NotifyRoutine(string message)
    {
        notifyText.text = message;
        notifyText.gameObject.SetActive(true);

        yield return new WaitForSeconds(notifyDuration);

        notifyText.gameObject.SetActive(false); 
    }


    public GameObject[] selectionGlows; 

    public void SelectItem1() { SelectItem(10, 0, "Thuốc hồi máu"); }
    public void SelectItem2() { SelectItem(5, 1, "Thuốc stamina"); }
    public void SelectItem3() { SelectItem(8, 2, "Phi tiêu"); }

    private void SelectItem(int price, int index, string name)
    {
        selectedPrice = price;
        selectedItemName = name;

        if (selectionGlows == null || selectionGlows.Length == 0) return;

        for (int i = 0; i < selectionGlows.Length; i++)
        {
            if (selectionGlows[i] != null)
            {
                selectionGlows[i].SetActive(i == index);
            }
        }
        Debug.Log("Đã chọn: " + selectedItemName + " - Giá: " + selectedPrice);
    }


    public void ConfirmBuy()
    {
        if (selectedPrice == 0 || string.IsNullOrEmpty(selectedItemName))
        {
            ShowNotify("Vui lòng chọn vật phẩm!");
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            HeroKnight playerScript = playerObj.GetComponent<HeroKnight>();

            if (playerScript.SpendGold(selectedPrice))
            {
                UpdateGoldDisplay(playerScript.m_gold);
                ShowNotify("Mua " + selectedItemName + " thành công!");

            }
            else
            {
                ShowNotify("Bạn không đủ vàng!");
            }
        }
    }

    public void UpdateGoldDisplay(int currentGold)
    {
        if (goldDisplayText != null)
        {
            goldDisplayText.text = "Vàng hiện có : " + currentGold;
        }
    }
}