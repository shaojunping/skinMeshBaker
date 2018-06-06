参考：
https://connect.unity.com/p/render-crowd-of-animated-characters?signup=true 
https://github.com/chenjd/Render-Crowd-Of-Animated-Characters

注意几点： 
mesh不太像例子里面一样有scale，这样导致选中物体却找不到物体，因为被缩放为0.01； 
动画需要是legacy 
gpu instance需要opengl es3.0； 
注意在shader里面勾选enable instance选项； 
贴图格式为rgbhalf，每个通道16位，增加精度以保存坐标； 
shader里面从贴图中采样要用tex2dLod确保采样的是mipmap第0级，因为这里的贴图不是普通意义上的贴图，而主要作用是存放信息。

另外，这个代码有些问题，color里面直接存放坐标，坐标为负数怎么办 
所以博主对其进行了修改，先获取所有帧，所有顶点的最小值和最大值，然后每个顶点存放相应的0到1之间的比值。同时，需要把shader里面传入顶点坐标的最大值、最小值。
增加拖拽。
