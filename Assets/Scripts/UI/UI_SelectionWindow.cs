using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace moon
{
    public interface ISelectable
    {
        public GameObject Prefab { get; }
    }

    public interface ISelectionHandler<T>
    {
        public Task<T> Completion { get;  }
        public List<T> SelectableItems { get; }
        public T SelectedItem { get; }
        public void Select(T item); 
    }

    public class NumberSelection : ISelectionHandler<int>
    {
        TaskCompletionSource<int> completion;
        public Task<int> Completion => completion.Task;
        public List<int> SelectableItems { get; private set; }
        public int SelectedItem { get; private set; }

        public NumberSelection(int min, int max)
        {
            Game.CurrentGame.SelectionWindow.Select(min, max); 
        }
        
        public void Select(int i)
        {
            SelectedItem = i;
            Game.CurrentGame.SelectionWindow.Close();
            completion.SetResult(i);
        }
    }

    public class Selection<T> : ISelectionHandler<T> where T : ISelectable
    {
        TaskCompletionSource<T> completion;
        public Task<T> Completion => completion.Task; 
        public List<T> SelectableItems { get; private set; }
        public T SelectedItem { get; private set; }

        public Selection(Player player, IEnumerable<T> items)
        {
            SelectableItems = new(items);
            completion = new();

            Game.CurrentGame.SelectionWindow.Select(this);
        }

        public void Select(T item)
        {
            SelectedItem = item;
            Game.CurrentGame.SelectionWindow.Close();
            completion.SetResult(item); 
        }
    }

    public class UI_SelectionWindow : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] GameObject window;
        [SerializeField] Transform itemArea, numberArea;
        [SerializeField] Button passButton; 

        public void Select<T>(Selection<T> selection) where T : ISelectable
        {
            window.SetActive(true);
            itemArea.gameObject.SetActive(true);
            numberArea.gameObject.SetActive(false);

            foreach (T item in selection.SelectableItems)
            {
                GameObject _item = Instantiate(item.Prefab, itemArea); 
                _item.AddComponent<Button>().onClick.AddListener(() => selection.Select(item));

                // Make sure we properly configure our Card Selections. 
                if(_item.TryGetComponent(out UI_Card ui))
                    ui.Setup(item as ICard); 
            }
        }

        public void Select(int min, int max)
        {
            InputField input = numberArea.GetComponent<InputField>();
            Button reduceButton = numberArea.GetComponentsInChildren<Button>().First(button => button.GetComponentInChildren<TextMeshProUGUI>().text == "-");
            Button increaseButton = numberArea.GetComponentsInChildren<Button>().First(button => button.GetComponentInChildren<TextMeshProUGUI>().text == "+");

            window.SetActive(true);
            itemArea.gameObject.SetActive(false); 
            numberArea.gameObject.SetActive(true);

            input.text = min.ToString(); 

            input.onEndEdit.AddListener(numb =>
            {
                int n = int.Parse(numb);

                if (n < min)
                    input.text = min.ToString();
                if (n > max)
                    input.text = max.ToString(); 
            });

            increaseButton.onClick.AddListener(() => {
                int n = int.Parse(input.text) + 1;
                if (n <= max)
                    input.text = n.ToString();
                reduceButton.interactable = n > min;
                increaseButton.interactable = n < max;
            });

            reduceButton.onClick.AddListener(() => {
                int n = int.Parse(input.text) - 1;
                if (n >= min)
                    input.text = n.ToString();
                reduceButton.interactable = n > min;
                increaseButton.interactable = n < max;
            });
        }

        public void SetTitle(string title) => this.title.text = title; 

        public void Close()
        {
            window.SetActive(false);

            foreach (Transform t in itemArea)
                Destroy(t.gameObject); 
        }
    }
}
