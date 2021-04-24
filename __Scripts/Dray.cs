using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
{
    public enum EMode
    {
        idle,
        move,
        attack,
        transition,
        knockback
    }

    [Header("Set in Inspector")]
    public float speed = 5;
    public float attackDuration = 0.25f; // Продолжительность атаки в секундах
    public float attackDelay = 0.5f; // Задержка между атаками
    public float transitionDelay = 0.5f; // Задержка перехода между комнатами
    public int maxHealth = 10;
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;

    [Header("Set Dynamically")]
    public int dirHeld = -1; // Направление, соответствующие удерживаемой клавише
    public int facing = 0; // Напрпавление движения Дрея
    public EMode mode = EMode.idle;
    public int numKeys = 0;
    public bool invincible = false;
    public bool hasGrappler = false;
    public Vector3 lastSafeLoc;
    public int lastSafeFacing;

    [SerializeField]
    private int _health;

    public int Health
    {
        get => _health;
        set => _health = value;
    }

    private float timeAtkDone = 0; // время когда должна завершаться анимация атаки
    private float timeAtkNext = 0; // время когда Дрей сможет повторить атаку

    private float transitionDone = 0;
    private Vector2 transitionPos;
    private float knockbackDone = 0;
    private float invincibleDone = 0;
    private Vector3 knockbackVel;

    private SpriteRenderer sRend;
    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;
    private Vector3[] directions = new Vector3[] { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
    private KeyCode[] keys = new KeyCode[] { KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow };

    private void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
        Health = maxHealth;
        lastSafeLoc = transform.position; // Начальная позиция безопасна
        lastSafeFacing = facing;
    }

    private void Update()
    {
        // Проверить состояние неуязвимости и необходимость выполнить отбрасывание
        if (invincible && Time.time > invincibleDone)
            invincible = false;
        sRend.color = invincible ? Color.red : Color.white;
        if (mode == EMode.knockback)
        {
            rigid.velocity = knockbackVel;
            if (Time.time < knockbackDone)
                return;
        }

        // Остановить Дрея до перемещения через дверь
        if (mode == EMode.transition)
        {
            rigid.velocity = Vector3.zero;
            anim.speed = 0;
            RoomPos = transitionPos; //  Оставить Дрея на месте
            if (Time.time < transitionDone)
                return;
            mode = EMode.idle;
        }

        //----Обработка ввода с клавиатуры и управление режимами eMode----
        dirHeld = -1;
        for (int i=0; i<4; i++)
        {
            if (Input.GetKey(keys[i]))
                dirHeld = i;
        }

        // Нажата клавиша атаки
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= timeAtkNext)
        {
            mode = EMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + attackDelay;
        }

        // Завершить атаку если время истекло
        if (Time.time >= timeAtkDone)
            mode = EMode.idle;

        // Выбрать правильный режим если Дрей не атакует
        if (mode != EMode.attack)
        {
            if (dirHeld == -1)
                mode = EMode.idle;
            else
            {
                facing = dirHeld;
                mode = EMode.move;
            }
        }

        //---- Действия в текущем режиме----
        Vector3 vel = Vector3.zero;
        switch (mode)
        {
            case EMode.attack:
                anim.CrossFade("Dray_Attack_" + facing, 0);
                anim.speed = 0;
                break;
            case EMode.idle:
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 0;
                break;
            case EMode.move:
                vel = directions[dirHeld];
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 1;
                break;
        }
        rigid.velocity = vel * speed;

        //  Завершение игры при нулевом здоровье
        if (Health <= 0)
            SceneManager.LoadScene(1); ;
    }

    private void LateUpdate()
    {
        // Получить координаты узла  сетки с размером  ячейки в половину единицы, ближайшего к данному персонажу
        Vector2 rPos = GetRoomPosOnGrid(0.5f);

        // Персонаж находится на плитке с дверью?
        int doorNum;
        for (doorNum = 0; doorNum < 4; doorNum++)
        {
            if (rPos == InRoom.DOORS[doorNum])
                break;
        }

        if (doorNum > 3 || doorNum != facing)
            return;

        // Перейти в следующую комнату
        Vector2 rm = RoomNum;
        switch (doorNum)
        {
            case 0:
                rm.x += 1;
                break;
            case 1:
                rm.y += 1;
                break;
            case 2:
                rm.x -= 1;
                break;
            case 3:
                rm.y -= 1;
                break;
        }

        // Проверить можно ли выполнить  переход в комнату rm
        if (rm.x >= 0 && rm.x <= InRoom.MAX_RM_X)
        {
            if (rm.y >= 0  && rm.y <= InRoom.MAX_RM_Y)
            {
                RoomNum = rm;
                transitionPos = InRoom.DOORS[(doorNum + 2) % 4];
                RoomPos = transitionPos;
                lastSafeLoc = transform.position;
                lastSafeFacing = facing;
                mode = EMode.transition;
                transitionDone = Time.time + transitionDelay;
            }
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (invincible)
            return;
        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
        if (dEf == null)
            return;

        Health -= dEf.damage;
        invincible = true;
        invincibleDone = Time.time + invincibleDuration;

        if (dEf.knockback) // Выполнить отбрасывание
        {
            // Определить направление отбрасывания
            Vector3 delta = transform.position - coll.transform.position;
            if (Math.Abs(delta.x) >= Math.Abs(delta.y))
            {
                // Отбрасывание по горизонтали
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            }
            else
            {
                // Отбрасывание по вертикали
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }

            // Применить скорость отскока к компоненту Rigidbody
            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;

            // Установить режим knockback и время прекращения отбрасывания
            mode = EMode.knockback;
            knockbackDone = Time.time + knockbackDuration;
        }
    }

    private void OnTriggerEnter(Collider colld)
    {
        PickUp pup = colld.GetComponent<PickUp>();
        if (pup == null)
            return;
        switch(pup.itemType)
        {
            case PickUp.EType.health:
                Health = Mathf.Min(Health + 2, maxHealth);
                break;
            case PickUp.EType.key:
                KeyCount++;
                break;
            case PickUp.EType.grappler:
                hasGrappler = true;
                break;
        }

        Destroy(colld.gameObject);
    }

    public void ResetInRoom(int healthLoss = 0)
    {
        transform.position = lastSafeLoc;
        facing = lastSafeFacing;
        Health -= healthLoss;

        invincible = true; // Сделать Дрея неуязвимым
        invincibleDone = Time.time + invincibleDuration;
    }

    // Реализация интерфейса IFacingMover
    public int GetFacing()
    {
        return facing;
    }

    public bool Moving => mode == EMode.move;

    public float GetSpeed()
    {
        return speed;
    }

    public float GridMult => inRm.gridMult;

    public Vector2 RoomPos 
    {
        get => inRm.RoomPos; 
        set => inRm.RoomPos = value; 
    }
    public Vector2 RoomNum 
    {
        get => inRm.RoomNum;
        set => inRm.RoomNum = value; 
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return inRm.GetRoomPosOnGrid(mult);
    }
    
    // Реализация интерфейса IKeyMaster
    public int KeyCount 
    {
        get => numKeys;
        set => numKeys = value; 
    }
}
