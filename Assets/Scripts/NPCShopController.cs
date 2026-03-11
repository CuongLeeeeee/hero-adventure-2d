using System.Collections;
using Assets.Scripts;
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
    //real
    public void ToggleShop()
    {
        notifyText.text = "";
        notifyText.gameObject.SetActive(false);

        if (shopPanel == null) return;
        bool isActive = !shopPanel.activeSelf;

        shopPanel.SetActive(isActive);


        if (goldUI != null)
        {
            goldUI.SetActive(isActive);
        }


        if (isActive)
        {
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

        if (itemName == "Health Potion") price = 10;
        else if (itemName == "Damage Potion") price = 20;
        else if (itemName == "Dart") price = 30;

        HeroKnight player = FindFirstObjectByType<HeroKnight>();
        if (player != null && player.SpendGold(price))
        {
            ShowNotify("Purchased " + itemName + " successfully!");
        }
        else
        {
            ShowNotify("Not enough gold to buy " + itemName + "!");
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

    public void SelectItem1() { SelectItem(10, 0, "Health Potion"); }
    public void SelectItem2() { SelectItem(20, 1, "Damage Potion"); }
    public void SelectItem3() { SelectItem(30, 2, "Dart"); }

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
            ShowNotify("Please select an item!");
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            HeroKnight playerScript = playerObj.GetComponent<HeroKnight>();

            if (selectedItemName == "Health Potion")
            {
                if (playerScript.currentHealth >= 100)
                {
                    ShowNotify("HP is full, cannot purchase!");
                }
            }


            if (playerScript.SpendGold(selectedPrice))
            {
                ApplyItemEffect(playerScript, selectedItemName);

                UpdateGoldDisplay(playerScript.m_gold);
                if (selectedItemName == "Dart" && GameHUDManager.Instance != null)
                {
                    GameHUDManager.Instance.UpdateDartCount(playerScript.m_dartCount);
                }
                ShowNotify("Purchased " + selectedItemName + " successfully!");
            }
            else
            {
                ShowNotify("Not enough gold!");
            }
        }
    }

    private void ApplyItemEffect(HeroKnight player, string itemName)
    {
        switch (itemName)
        {
            case "Health Potion":
                player.RestoreHealth(10);
                break;
            case "Damage Potion":
                player.IncreaseDamage(5);
                break;
            case "Dart":
                player.AddDarts(5);
                break;
        }
    }

    public void UpdateGoldDisplay(int currentGold)
    {
        if (goldDisplayText != null)
        {
            goldDisplayText.text = "Current Gold: " + currentGold;
        }
    }
}