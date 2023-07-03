// This interface will be implemented from every object that has some property that has to be saved
public interface IDataPersistence
{
    public void LoadData(GameData gameData, bool isNewGame);
	public void SaveData(ref GameData gameData);
}
