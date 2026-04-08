public class PokemonInstance
{
    public PokemonData rootData;
    public PokemonData data;

    public int enhanceLevel;

    public Gender gender;
    public bool isShiny;

    public PokemonInstance(PokemonData data, Gender gender, bool isShiny)
    {
        this.rootData = data;
        this.data = data;
        this.gender = gender;
        this.isShiny = isShiny;
        enhanceLevel = 0;
    }
}