using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Entity
{


    
    // implement an Entity Component System
    // "ECS"

    // this means that the game does not have to be built out of lots of confusing heirarchies
    // or inherited traits but is made up of composition of components.

    // the ECS then can evaluate the type of an entity based on which components it has and
    // can use a "____handler" to handle those types of entities.

    // for example, if we are dealing with enemies, I may have a handler which handles all the 
    // enemies that have position, velocity, attacks, etc. But I would not use the same handler
    // to handle the enemies who do not have velocity (i.e. a stationary enemy object)


    // will have texture array containing the appropriate textures for the entities so that
    // they can be accessed via index rather than via storing the texture in each entity


    
}
