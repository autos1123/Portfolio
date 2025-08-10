using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;


public enum Skillinput
{
    None = -1, // 기본값, 아이템이 없는 상태
    X = 0,
    C = 1,
}
public class PlayerActiveItemController:MonoBehaviour
{
    private Dictionary<SkillType, ISkillExecutor> executors;
    private PlayerController playerController;
    private ActiveItemEffectDataTable activeItemEffectDataTable;

    public List<ActiveItemData> activeItemDatas = new();
    public List<float> activeItemCoolTime = new();
    public List<Action<float>> OnActiveItemCoolTime = new();

    [SerializeField] private Transform projectileSpawnPos;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        activeItemEffectDataTable = TableManager.Instance.GetTable<ActiveItemEffectDataTable>();
        executors = new Dictionary<SkillType, ISkillExecutor>
        {
            { SkillType.Projectile, new ProjectileSkillExecutor() },
            { SkillType.AoE, new AoESkillExecutor() },
            { SkillType.Heal, new HealSkillExecutor() },
            { SkillType.Zone, new ZoneSkillExecutor() },
        };
    }

    private void Update()
    {
        // 예시용 단축키(테스트)
        if(Input.GetKeyDown(KeyCode.F3))
        {
            UseExecutorById(5000, projectileSpawnPos);
        }
        if(Input.GetKeyDown(KeyCode.F4))
        {
            UseExecutorById(5001, projectileSpawnPos);
        }
        if(Input.GetKeyDown(KeyCode.F5))
        {
            UseExecutorById(5002, transform);
        }
        if(Input.GetKeyDown(KeyCode.F6))
        {
            UseExecutorById(5003, projectileSpawnPos);
        }
    }

    // 기존 함수 유지
    public void TakeItem(ActiveItemData activeItemData)
    {
        activeItemDatas.Add(activeItemData);
    }

    public void TakeItem(Skillinput skillinput, ActiveItemData activeItemData)
    {
        int index = (int)skillinput;
        while(activeItemDatas.Count <= index)
        {
            activeItemDatas.Add(null);
            activeItemCoolTime.Add(0);
        }
        activeItemDatas[index] = activeItemData;
        activeItemCoolTime[index] = 0;
    }

    public void UseItem(Skillinput skillinput)
    {
        int index = (int)skillinput;
        
        var used = activeItemEffectDataTable.GetDataByID(activeItemDatas[index].skillID);
        activeItemCoolTime[index] = used.Cooldown;
        GameEvents.TriggerActiveSkillUse();
        StartCoroutine(CoolDown(index));
        Transform caster = used.Type == SkillType.Heal ? transform : projectileSpawnPos;
        executors[used.Type].Execute(used, caster);
    }

    public bool CanUseSkill(Skillinput skillinput)
    {
        int index = (int)skillinput;
        if(index < 0 || index >= activeItemDatas.Count)
        {
            Debug.LogWarning($"잘못된 스킬 인덱스: {index}");
            return false;
        }
        if(activeItemDatas[index] == null)
            return false;
        if(activeItemCoolTime[index] > 0) // ★ 쿨타임 체크는 '>'가 더 일반적
            return false;

        return true;
    }



    IEnumerator CoolDown(int index)
    {
        while(activeItemCoolTime[index] >= 0)
        {
            activeItemCoolTime[index] -= Time.deltaTime;
            float tempCoolTime = activeItemCoolTime[index] / activeItemEffectDataTable.GetDataByID(activeItemDatas[index].skillID).Cooldown;
            OnActiveItemCoolTime[index]?.Invoke(tempCoolTime);
            yield return null;
        }
    }

    // 테스트용 직접 ID 실행 (단축키)
    private void UseExecutorById(int id, Transform caster)
    {
        var used = activeItemEffectDataTable.GetDataByID(id);
        if(used != null && executors.TryGetValue(used.Type, out var executor))
        {
            executor.Execute(used, caster);
        }
        else
        {
            Debug.LogWarning($"SkillType {used?.Type}에 대한 Executor가 없습니다.");
        }
    }
}
