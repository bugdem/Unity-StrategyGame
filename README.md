# Unity-StrategyGame

A strategy game demo that demonstrates various techniques of OOP regarding to GDD.

- SpriteAtlas is used for the tiles and character animations to reduce the draw calls (ScrollView's mask adds more than 10 setpass call, so to make it under 20 call, mask can be disabled).
- Game is designed with new Input System logic is abstracted.
- Canvas objects are adjusted to fit different screen sizes.
- Productions and soldiers(Production items) are defined as ScriptableObjects.
  If new buildings or units are required, they can be added by creating new ScriptableObjects and added to BoardSetting SO.
- Productions are instant and can be created without any cost.

- Camera pan and focus on the productions are implemented.
- Game is developed with Design Pattnerns, SOLID and DRY principles in mind.

  Logic :
- BoardController is responsible for boad operations and game flow mostly.
- BoardSetting is holds the data of the game. It is a ScriptableObject and can be created from the Assets/Create/Game Engine/BoardSetting menu.
- BoardGrid is used for the grid operations and pathfinding.

  How to Play :
- Productions can be dragged and dropped to the grid.
- Soldiers can be created by clicking on the production items.
- Productions on board can be selected with left click and move/attack with right click.

* There is an Enemy Unit and Building in the scene to test attack and destroy.
