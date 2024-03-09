
A strategy game demo that demonstrates various techniques of OOP regarding to GDD.

- SpriteAtlas is used for the tiles and character animations to reduce the draw calls.
- Game is designed with new Input System logic is abstracted.
- Canvas objects are adjusted to fit different screen sizes.
- Productions and soldiers(Production items) are defined as ScriptableObjects.
  If new buildings or units are required, they can be added by creating new ScriptableObjects and added to BoardSetting SO.
- Productions are instant and can be created without any cost.
- Productions can be dragged and dropped to the grid.
- Camera pan and focus on the productions are implemented.
- Game is developed with Design Pattnerns, SOLID and DRY principles in mind.

  Logic :
- BoardController is responsible for boad operations and game flow mostly.
- BoardSetting is holds the data of the game. It is a ScriptableObject and can be created from the Assets/Create/Game Engine/BoardSetting menu.
- BoardGrid is used for the grid operations and pathfinding.