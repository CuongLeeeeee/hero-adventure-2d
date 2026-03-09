using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameHUDManager : MonoBehaviour
    {
        public static GameHUDManager Instance;

        [Header("UI References")]
        public TextMeshProUGUI dartCountText; // Kéo thả cái Text số phi tiêu vào đây

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Hàm này để các script khác (như HeroKnight) gọi khi số lượng phi tiêu thay đổi
        public void UpdateDartCount(int count)
        {
            if (dartCountText != null)
            {
                dartCountText.text = "x " + count;
            }
        }
    }
}
