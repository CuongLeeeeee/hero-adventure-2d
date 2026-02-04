using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCShopController : MonoBehaviour
{
    [Header("UI Settings")]
    public Text notifyText; // Kéo đối tượng NotifyText vào đây
    public float notifyDuration = 2.0f; // Thời gian hiện thông báo
    private string selectedItemName = ""; // Thêm biến này để nhớ tên món đang chọn

    public GameObject[] highlightFrames; // Kéo 3 khung Highlight vào mảng này theo đúng thứ tự 0, 1, 2
    public TextMeshProUGUI goldDisplayText; // Dòng này phải là PUBLIC
    public GameObject shopPanel;
    private bool isNearNPC = false; // Biến kiểm tra xem có đang đứng gần NPC không
    private int selectedPrice = 0; // Lưu giá món đồ đang chọn
    public GameObject goldUI; // Đối tượng chứa cả Text và Icon vàng
    void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    void Update()
    {
        // PHẢI CÓ biến check va chạm (isNearNPC) ở đây
        if (Input.GetKeyDown(KeyCode.F) && isNearNPC)
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        if (shopPanel == null) return;
        bool isActive = !shopPanel.activeSelf; // Đảo ngược trạng thái hiện tại

        shopPanel.SetActive(isActive); // Ẩn/Hiện bảng Shop

        // Nếu bạn đã kéo GoldGroup vào ô Gold UI, dòng này sẽ ẩn/hiện nó theo Shop
        if (goldUI != null)
        {
            goldUI.SetActive(isActive);
        }

        // Logic xử lý khi mở Shop (isActive == true)
        if (isActive)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            // Cập nhật số vàng ngay khi hiện lên
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

        if (!isActive) // Nếu Shop vừa bị đóng
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
            // Hiện thông báo kèm tên đối tượng chạm vào để kiểm tra chính xác
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

        // Tự động xác định giá dựa trên tên vật phẩm
        if (itemName == "Mau") price = 10;
        else if (itemName == "Stamina") price = 5;
        else if (itemName == "PhiTieu") price = 8;

        HeroKnight player = FindFirstObjectByType<HeroKnight>();
        if (player != null && player.SpendGold(price))
        {
            ShowNotify("Đã mua " + itemName + " thành công!");
            // Tại đây bạn có thể thêm logic cộng vật phẩm vào túi đồ (Inventory)
        }
        else
        {
            ShowNotify("Không đủ vàng để mua " + itemName + "!");
        }
    }

    // Logic xử lý hiện/ẩn thông báo
    public void ShowNotify(string message)
    {
        StopAllCoroutines(); // Xóa thông báo cũ nếu đang hiện dở
        StartCoroutine(NotifyRoutine(message));
    }

    IEnumerator NotifyRoutine(string message)
    {
        notifyText.text = message;
        notifyText.gameObject.SetActive(true); // Hiện chữ

        yield return new WaitForSeconds(notifyDuration);

        notifyText.gameObject.SetActive(false); // Ẩn chữ
    }


    public GameObject[] selectionGlows; // Kéo 3 cái Viền Vàng vào đây

    public void SelectItem1() { SelectItem(10, 0, "Thuốc hồi máu"); }
    public void SelectItem2() { SelectItem(5, 1, "Thuốc stamina"); }
    public void SelectItem3() { SelectItem(8, 2, "Phi tiêu"); }

    private void SelectItem(int price, int index, string name)
    {
        selectedPrice = price;
        selectedItemName = name; // Ghi nhớ tên món đồ

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


    // Hàm này gắn vào duy nhất nút MUA to dưới cùng
    public void ConfirmBuy()
    {
        // Kiểm tra xem đã chọn món nào chưa
        if (selectedPrice == 0 || string.IsNullOrEmpty(selectedItemName))
        {
            ShowNotify("Vui lòng chọn vật phẩm!");
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            HeroKnight playerScript = playerObj.GetComponent<HeroKnight>();

            // Sử dụng hàm SpendGold đã có trong HeroKnight để trừ tiền
            if (playerScript.SpendGold(selectedPrice))
            {
                // Mua thành công
                UpdateGoldDisplay(playerScript.m_gold);
                ShowNotify("Mua " + selectedItemName + " thành công!");

                // Tùy chọn: Sau khi mua có thể xóa lựa chọn hoặc giữ nguyên viền
                // selectedPrice = 0; 
                // selectedItemName = "";
            }
            else
            {
                // Không đủ tiền
                ShowNotify("Bạn không đủ vàng!");
            }
        }
    }

    // Tạo thêm hàm này để dùng chung cho việc cập nhật chữ
    public void UpdateGoldDisplay(int currentGold)
    {
        if (goldDisplayText != null)
        {
            // Thay đổi nội dung hiển thị ở đây
            // Lưu ý: Chúng ta không cần ghi chữ "Icon" vào code vì Icon đã là một Image riêng bên cạnh rồi
            goldDisplayText.text = "Vàng hiện có : " + currentGold;
        }
    }
}
//11123 