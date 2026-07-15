using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    // Variável global em memória para saber se o próximo nível deve reposicionar o jogador a partir do Save
    public static bool carregarSaveAoIniciar = false;

    // ---------------------------------------------
    //  GUARDAR PROGRESSO
    // ---------------------------------------------
    public static void GuardarProgresso()
    {
        // 1. Guarda unicamente a cena ativa
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", currentScene);

        // Grava fisicamente no disco/registro do Windows
        PlayerPrefs.Save();
        Debug.Log($"💾 [SaveSystem] Progresso do jogo guardado com sucesso (Apenas a cena: {currentScene})!");
    }

    // ---------------------------------------------
    //  APLICAR SAVE AO INICIAR A CENA (MÉTODO SIMPLIFICADO)
    // ---------------------------------------------
    public static void AplicarSaveAoPlayer(GameObject player)
    {
        // Sistema simplificado: Como apenas guardamos a cena, o jogador faz spawn natural nas posições padrão
        carregarSaveAoIniciar = false;
    }

    // ---------------------------------------------
    //  LIMPAR SAVE ANTERIOR (Novo Jogo)
    // ---------------------------------------------
    public static void LimparSaveProgresso()
    {
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.DeleteKey("PlayerKeys"); // Limpa o inventário acumulado para um Novo Jogo do zero
        PlayerPrefs.Save();
        Debug.Log("🗑️ [SaveSystem] Registro de save de cena e chaves limpo.");
    }
}
