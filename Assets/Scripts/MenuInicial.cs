using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuInicial : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Jugar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Update is called once per frame
    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}
