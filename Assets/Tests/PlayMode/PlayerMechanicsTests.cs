using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Тестовая сборка PlayModeTests (includePlatforms не указан) — тесты PlayMode
public class PlayerMechanicsTests
{
    // PLAYMODE ТЕСТ (базовый): игрок, выпавший за границу мира, возрождается в точке спавна
    [UnityTest]
    public IEnumerator PlayMode_FallBelowWorld_RespawnsAtSpawnPoint()
    {
        // Создаем менеджер игры: Awake задает Instance и точку спавна, Start строит уровень
        var gmGo = new GameObject("GameManager");
        gmGo.AddComponent<GameManager>();
        yield return null; // даём Start() отработать

        // Временный игрок с реальным скриптом из игры
        var player = new GameObject("TestPlayer");
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<CapsuleCollider2D>();
        player.AddComponent<PlayerController>();

        // Бросаем игрока за границу мира (в Update порог: y < -8)
        player.transform.position = new Vector3(0f, -10f, 0f);
        yield return null; // кадр, на котором Update ловит падение и вызывает Respawn
        yield return null; // кадр на применение физики

        Assert.Greater(player.transform.position.y, -8f,
            "Ошибка: игрок должен возродиться, а не оставаться за границей мира!");
        Assert.AreEqual(-12f, player.transform.position.x, 0.1f,
            "Ошибка: игрок должен появиться в точке спавна (x = -12)!");

        Object.Destroy(player);
        Object.Destroy(gmGo);
        yield return null;
    }

    // PLAYMODE ТЕСТ (ЛР №4, ВАРИАНТ 8): при controlEnabled = true ввод меняет направление движения
    [UnityTest]
    public IEnumerator PlayMode_ControlEnabledTrue_InputChangesMoveDirection()
    {
        // Создаем временный объект и добавляем ему реальный скрипт управления из игры
        var playerObj = new GameObject("TestPlayer");
        playerObj.AddComponent<Rigidbody2D>(); // Awake контроллера требует Rigidbody2D
        PlayerController player = playerObj.AddComponent<PlayerController>();

        // Ввод разрешен
        player.controlEnabled = true;
        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();

        // Имитируем нажатие «вправо» (клавиша D): игрок должен поехать вправо
        player.ApplyInput(1f);
        Assert.Greater(rb.velocity.x, 0f,
            "Ошибка: при controlEnabled = true ввод «вправо» должен двигать игрока вправо!");
        float speedRight = rb.velocity.x;

        // Имитируем нажатие «влево» (клавиша A): направление движения должно смениться
        player.ApplyInput(-1f);
        Assert.Less(rb.velocity.x, 0f,
            "Ошибка: ввод должен менять направление движения игрока!");
        Assert.AreEqual(-speedRight, rb.velocity.x, 0.001f,
            "Ошибка: скорость влево должна быть зеркальной скорости вправо!");

        // Контрольная проверка механики: controlEnabled = false обнуляет горизонтальный ввод
        player.controlEnabled = false;
        player.ApplyInput(1f);
        Assert.AreEqual(0f, rb.velocity.x, 0.001f,
            "Ошибка: при controlEnabled = false горизонтальный ввод должен игнорироваться!");

        // Удаляем объект после теста, чтобы очистить память
        Object.Destroy(playerObj);
        yield return null;
    }
}
