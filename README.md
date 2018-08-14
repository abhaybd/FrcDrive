# FrcDrive
This is a driving sim I made to test my swerve drive implementation. It also requires RPC code on the other end. I probably screwed up with how I implemented RPC, but whatever. The way I set it up, it uses a shitton of reflection on the Java end so I don't have to pretty much recreate the entire library on the Unity end.

The FRC driving code is on a branch of my forked repo of my team's (Titan Robotics 492) driving code. The branch is available [here.](https://github.com/coolioasjulio/Frc2018FirstPowerUp/tree/SwerveDrive-Testing) The branch is called SwerveDrive-Testing, because it has all the testing code that doesn't go on the robot.

## Controls
- Turn clockwise: e
- Turn counter-clockwise: q
- Move right: d
- Move left: a
- Move forward: w
- Move backward: s

Due to the *radical and gnarly* nature of swerve drive, all of this actually happens **relative to the camera** rather than relative to the robot, so you don't have to consider the robot's orientation. It's pretty cool.
