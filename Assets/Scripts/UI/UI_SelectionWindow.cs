using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;

namespace moon
{
    public interface ISelectable
    {
        public GameObject Prefab { get; }
    }

    public class Selection<T> where T : ISelectable
    {
        TaskCompletionSource<T> completion;
        public Task<T> Completion => completion.Task; 
        public List<T> SelectableItems { get; private set; }
        public T SelectedItem { get; private set; }

        public Selection(IEnumerable<T> items)
        {
            SelectableItems = new(items);
            completion = new();

            Game.SelectionWindow.Select(this);
        }

        public void Select(T item)
        {
            SelectedItem = item;
            Game.SelectionWindow.Close();
            completion.SetResult(item); 
        }
    }

    public class UI_SelectionWindow : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] GameObject window;
        [SerializeField] Transform itemArea;
        [SerializeField] Button passButton; 

        public void Select<T>(Selection<T> selection) where T : ISelectable
        {
            window.SetActive(true); 

            foreach(T item in selection.SelectableItems)
            {
                GameObject _item = Instantiate(item.Prefab, itemArea); 
                _item.AddComponent<Button>().onClick.AddListener(() => selection.Select(item));
            }
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
