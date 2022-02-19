# Procedural Animation Demo

## Effect

![](https://s4.ax1x.com/2022/02/19/Hbzu11.gif)

## Introduce

Motion animation is procedurally controlled, using the principles of inverse kinematics.

Each leg of the spider monster is calculated using the fabrik algorithm.

## Fabrik

Each iteration is divided into two, one forward and one reverse. The process of each iteration is as follows:

- First, for a bone chain from the beginning to the end, and a corresponding Target point, first calculate the length of the entire chain to determine whether the Target point is reachable.
  - If not reachable, ask the root bone to move. The usual situation is similar to when a person stretches his hand or a giraffe stretches his neck and cannot reach the target point, then he needs to move to the target point by himself (in the environment of Jacobian matrix calculation, this is When the matrix is ​​irregular, no corresponding inverse can be calculated).
  - If it can be reached, it means that the next step can be processed. Here, it is assumed that there are a total of five bones (from the root bone to the end bones are p0p0, p1p1, p2p2, p3p3, p4p4), and the target position (t ).

- If the entire bone chain is guaranteed to be within reach, then the FABRIK process can begin. The whole is divided into two steps. First, start the calculation from the end bone, and first move the endmost bone p4p4 to the target position t. At this time, the position of the bone p4p4 is p′4p4′.

- Then, connect p3p3 and p'4p4' into a straight line, and pull the current p3p3 to p'3p3' at the same distance as p'4p4' through the original distance between p3p3 and p4p4. Process all the way to the root bone p0p0

- After that, start processing from the root bone p'0p0'. Since the root bone is immobile by default in the entire iteration process, the root bone p''0p0" is moved to the original position p0p0, and then the same distance constraint is used until the tail bone p4p4 is processed.

- Repeat the above iterative process until the final tail bone position reaches the target position (or the distance from the target position is less than a predetermined value) to stop.

![](https://img-blog.csdn.net/20180506163521466)
