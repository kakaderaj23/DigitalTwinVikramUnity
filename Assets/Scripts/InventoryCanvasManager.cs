using UnityEngine;
using UnityEngine.UI;

public class InventoryManagerCanvas : MonoBehaviour
{
    public GameObject inventoryWindow;
    public Button openInventoryButton;
    public Button closeInventoryButton;
    public GameObject openInventory;
    public GameObject closeInventory;
    public bool isInventoryOpen = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        openInventoryButton.onClick.AddListener(OpenInventory);
        closeInventoryButton.onClick.AddListener(CloseInventory);
        openInventory.SetActive(true);
        closeInventory.SetActive(false);
        inventoryWindow.SetActive(false);
    }
    void OpenInventory()
    {
        inventoryWindow.SetActive(true);
        openInventory.SetActive(false);
        closeInventory.SetActive(true);
        isInventoryOpen = true;
    }
    void CloseInventory()
    {
        inventoryWindow.SetActive(false);
        openInventory.SetActive(true);
        closeInventory.SetActive(false);
        isInventoryOpen = false;
    }
}
