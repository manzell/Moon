using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace moon
{
    public class UI_SelectionWindow : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;

        public TaskCompletionSource<IIcon> Selection { get; private set; } = new(); 

        public void StartSelection()
        {
            foreach(Button button in GetComponentsInChildren<Button>())
                button.onClick.AddListener(() => Selection.SetResult(button.GetComponent<UI_Icon>().Icon)); 
        }

    }
}
