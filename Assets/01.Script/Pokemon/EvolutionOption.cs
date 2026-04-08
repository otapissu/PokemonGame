using UnityEngine;
using System;

[Serializable]
public class EvolutionOption
{
    public PokemonData targetData;
    public EvolutionMethod method;
    public EvolutionItemType requiredItem;

    [Tooltip("-1이면 현재 어떤 폼이든 허용, 0 이상이면 해당 폼일 때만 진화 가능")]
    public int requiredSourceFormIndex = -1;

    [Tooltip("랜덤이 아닐 때 진화 후 도착할 폼 인덱스")]
    public int targetFormIndex = 0;

    [Tooltip("레벨 진화일 때 필요한 강화 레벨")]
    public int requiredEnhanceLevel = 0;

    [Tooltip("진화체 폼 랜덤 여부")]
    public bool randomizeTargetForm = false;
}