using System.Collections.Generic;
using UnityEngine;


public class PokemonEvolutionService
{
    public bool HasEvolutionOption(PokemonInstance instance)
    {
        if (instance == null)
        {
            return false;
        }

        if (instance.data == null)
        {
            return false;
        }

        if (instance.data.evolutionOptions == null)
        {
            return false;
        }

        for (int i = 0; i < instance.data.evolutionOptions.Length; i++)
        {
            EvolutionOption option = instance.data.evolutionOptions[i];

            if (IsUsableOption(option))
            {
                return true;
            }
        }

        return false;
    }

    public List<EvolutionOption> GetAllUsableOptions(PokemonInstance instance)
    {
        List<EvolutionOption> result = new List<EvolutionOption>();

        if (instance == null)
        {
            return result;
        }

        if (instance.data == null)
        {
            return result;
        }

        if (instance.data.evolutionOptions == null)
        {
            return result;
        }

        for (int i = 0; i < instance.data.evolutionOptions.Length; i++)
        {
            EvolutionOption option = instance.data.evolutionOptions[i];

            if (!IsUsableOption(option))
            {
                continue;
            }

            result.Add(option);
        }

        return result;
    }

    public List<EvolutionOption> GetOptionsAtCurrentTiming(PokemonInstance instance)
    {
        List<EvolutionOption> result = new List<EvolutionOption>();
        List<EvolutionOption> allOptions = GetAllUsableOptions(instance);

        for (int i = 0; i < allOptions.Count; i++)
        {
            EvolutionOption option = allOptions[i];

            if (!IsMatchingSourceForm(option, instance))
            {
                continue;
            }

            if (!IsEnhanceLevelSatisfied(option, instance))
            {
                continue;
            }

            result.Add(option);
        }

        return result;
    }

    public bool IsAtAnyEvolutionTiming(PokemonInstance instance)
    {
        List<EvolutionOption> options = GetOptionsAtCurrentTiming(instance);
        return options.Count > 0;
    }

    public bool HasLevelEvolutionAtCurrentTiming(PokemonInstance instance)
    {
        List<EvolutionOption> options = GetOptionsAtCurrentTiming(instance);

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].method == EvolutionMethod.Level)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasItemEvolutionAtCurrentTiming(PokemonInstance instance)
    {
        List<EvolutionOption> options = GetOptionsAtCurrentTiming(instance);

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].method == EvolutionMethod.Item)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasRequiredItemForAnyCurrentEvolution(PokemonInstance instance)
    {
        List<EvolutionOption> options = GetOptionsAtCurrentTiming(instance);

        for (int i = 0; i < options.Count; i++)
        {
            EvolutionOption option = options[i];

            if (option.method != EvolutionMethod.Item)
            {
                continue;
            }

            if (HasItem(option.requiredItem))
            {
                return true;
            }
        }

        return false;
    }

    public List<EvolutionOption> GetAvailableEvolutionOptions(PokemonInstance instance)
    {
        List<EvolutionOption> result = new List<EvolutionOption>();
        List<EvolutionOption> options = GetOptionsAtCurrentTiming(instance);

        for (int i = 0; i < options.Count; i++)
        {
            EvolutionOption option = options[i];

            if (option.method == EvolutionMethod.Level)
            {
                result.Add(option);
                continue;
            }

            if (option.method == EvolutionMethod.Item)
            {
                if (HasItem(option.requiredItem))
                {
                    result.Add(option);
                }

                continue;
            }
        }

        return result;
    }

    public bool CanEvolveNow(PokemonInstance instance)
    {
        List<EvolutionOption> options = GetAvailableEvolutionOptions(instance);
        return options.Count > 0;
    }

    public EvolutionOption GetRandomAvailableEvolution(PokemonInstance instance)
    {
        List<EvolutionOption> availableOptions = GetAvailableEvolutionOptions(instance);

        if (availableOptions.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, availableOptions.Count);
        return availableOptions[randomIndex];
    }

    public bool TryEvolve( PokemonInstance instance, EvolutionOption selectedOption)
    {
        if (instance == null)
        {
            return false;
        }

        if (!IsUsableOption(selectedOption))
        {
            return false;
        }

        if (!IsMatchingSourceForm(selectedOption, instance))
        {
            return false;
        }

        if (!IsEnhanceLevelSatisfied(selectedOption, instance))
        {
            return false;
        }

        if (selectedOption.method == EvolutionMethod.Item)
        {
            PlayerEvolveInventory.Instance?.ConsumeItem(selectedOption.requiredItem);
            EvolveBagPanelController.Instance?.ClearSelection();
        }

        instance.data = selectedOption.targetData;

        if (selectedOption.randomizeTargetForm)
        {
            instance.formIndex = PokemonFormUtility.GetRandomFormIndex(instance.data, instance.gender, instance.isShiny);
        }
        else
        {
            instance.formIndex = Mathf.Max(0, selectedOption.targetFormIndex);
        }

        return true;
    }

    private bool IsUsableOption(EvolutionOption option)
    {
        if (option == null)
        {
            return false;
        }

        if (option.targetData == null)
        {
            return false;
        }

        if (option.method == EvolutionMethod.None)
        {
            return false;
        }

        return true;
    }

    private bool IsMatchingSourceForm(EvolutionOption option, PokemonInstance instance)
    {
        if (option == null)
        {
            return false;
        }

        if (instance == null)
        {
            return false;
        }

        if (option.requiredSourceFormIndex < 0)
        {
            return true;
        }

        return option.requiredSourceFormIndex == instance.formIndex;
    }

    private bool IsEnhanceLevelSatisfied(EvolutionOption option, PokemonInstance instance)
    {
        if (option == null)
        {
            return false;
        }

        if (instance == null)
        {
            return false;
        }

        return instance.enhanceLevel >= option.requiredEnhanceLevel;
    }

    private bool HasItem(EvolutionItemType itemType)
    {
        if (itemType == EvolutionItemType.None)
        {
            return false;
        }
        if (EvolveBagPanelController.Instance == null)
        {
            return false;
        }
        return EvolveBagPanelController.Instance.SelectedItemType == itemType;
    }
}