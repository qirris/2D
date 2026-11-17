using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlMenu : MonoBehaviour
{

    public void Jugar()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void VerCreditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    public void volverAlMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Salir()
    {
        Application.Quit();
    }


}
