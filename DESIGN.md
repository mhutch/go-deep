    If you love your dog, we're gonna mess with your
    mind, man. You're not going to be able to go to bed.
        - Peter Molyneux
        
http://www.molyjam.com/inspirations/15


    When you rip your fingers on the screen and you tear
    the landscape apart with your physical hands, it just
    feels amazing, man. It feels amazing.

http://www.molyjam.com/inspirations/20


### The Pitch

Your dog is running after a bone, and has followed it down a hole into the depths of the Earth. You must catch your dog before he gets lost in the tunnel and starves to death.

### Basic Design

#### Intro Screen

A “Start” button.

#### Intro Cinematic

Sets the scene. Hand throwing bone, bone sails through air. Dog barks, runs after it, disappears into hole in ground.

#### The Game

Running after your dog in a tunnel, avoiding obstacles, Temple Run style. You are slowly catching up with your dog but are slowed down by obstacles. If your dog gets too far away, you lose him.

Play it on touch devices, because some obstacles will need to be swiped out of the way.

There is no pause button, there is anti-pause. The position is based on time. If player closes game and returns later, it will have advanced and and player will most likely have lost.

### Ideas

* Your dog is a dachshund
* Type name for dog.
* Allow player to choose from more kinds of dog before game starts
* Portraits of lost dogs in intro screen
* Randomly generated dogs to choose from
* Different stages of tunnel, with different music, obstacles, walls etc
* Power ups, speeds boosts, etc
* Different kinds of obstacles
* Meter indicating how far away your dog is
* Scoring system
* Forks in the tunnel
* Different breeds of dog?
* Dog is also avoiding obstacles, barks to warn you.

### Cinematics

Simple timed fullscreen images.

#### Intro

Hand throws bone, dog barks, bone sails through air, dog runs past and disappears into hole. Whimsical music playing, music turns dark/creepy as dog disappears. Think Danny Elfman.

#### Win

Dog turns around, comes towards. Happy music. Hand pets dog.

#### Loss

Fade to black. Sad music tugging at heartstrings. Simple text on black background, name of dog and date of death.

### Tunnel

* Tunnel is 3D, faked by octagonal layers of sprites.
* Dog and obstacles are sprites.
* Layers move towards you at constant rate.
* Dog is rendered with simple running animation in center of tunnel in distance.
* Each octagonal section of each layer can have an obstacle on it. 
* Player gradually catches up with the dog.
* If player hits an obstacle, the tunnel stops moving briefly but the dog continues moving, so the dog gets further away.
* Player can tap to move move left/right and avoid obstacles. This simply rotates the octagon.
* Player can swipe to destroy some kinds of obstacle.
* Only show half of octagon in screen layer.

### Milestones

#### Programming

* ~~Set up program structure~~
* ~~Render tunnel structure~~
* ~~Render tunnel forwards movement~~
* ~~Render animated dog~~
* ~~Randomly generate tunnel sections~~
* ~~Render simple rock obstacles~~
* ~~Rotation gameplay mechanic~~
* ~~Implement obstacle collision and movement slowdown~~
* ~~Implement catching up/getting behind~~
* ~~Win/lose conditions~~
* ~~Music~~
* ~~Collision sounds~~
* ~~Intro cinematic~~
* ~~Win cinematic~~
* ~~Loss cinematic~~
* ~~Intro splash~~
* Speedup powerup
* Slowdown obstacle
* More graphics!
* Psychedelic sections

### Obstacles/Power-Ups

* Slow (ice? banana?)
* Go slower for a while, lighting turns blue
* Speed boost
* Go faster for a while, everything brightens up
* Wall (stone block)
* Bounce off it and around it
* Psychadelic
* Swap out tileset and music, warp perspective

### Needed Art

* Lots more wall tiles, maybe some variety, e.g. sandstone, moss, rock, etc
* Maybe grouped into several types, e.g. 5 dirt, 5 sandstone, 5 rock, etc?
* Power-ups/obstacles
* Slow
* Boost
* Wall
