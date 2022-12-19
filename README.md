# Practical-URP

URP下实用功能实现和效果积累，作为入门学习记录、生产模板积累，将一些老教程的案例复现在新环境下。

版本：2021.3.8f

### 内容

#### 后处理扫描效果（像素世界坐标 + 描边）

![image-20221206164719169](https://s2.loli.net/2022/12/07/n1GbXkudyNewVj5.png)

#### 深度卡通水（像素世界坐标 + 噪声采样）

![image-20221209150920125](https://s2.loli.net/2022/12/09/vwAMEfsznkbIQt7.png)



#### RenderMask+后处理热扭曲效果

![image-20221206164938766](https://s2.loli.net/2022/12/07/n4Y7UaRl3KfXCAO.png)

#### 不过滤透明对象的CameraColorTextureAlpha

![image-20221206164104351](https://s2.loli.net/2022/12/07/7GdyTRiQEW4zuMO.png)

#### Normalmap Glass

![image-20221207144704266](https://s2.loli.net/2022/12/07/tVguLaNpY71jZ2y.png)

#### 手动生成CameraDepthNormalsTexture

![image-20221206164224106](https://s2.loli.net/2022/12/07/hgreb6QjwPpVAxD.png)

#### 手动多光源处理

![image-20221206164319011](https://s2.loli.net/2022/12/07/aWTBoyb1ps69rG4.png)

#### BillBoard LensFlare

（假装这块装是辉光）

![image-20221206164443549](https://s2.loli.net/2022/12/07/pvj9iUIO6PeqNLK.png)

#### 深度图重构像素世界坐标

![image-20221206164532181](https://s2.loli.net/2022/12/07/n93GXvQxfg1OTUe.png)

#### 后处理模糊

分别为Radial, Kawase, Dual Kawase, Bokeh Blur

![image-20221206165026827](https://s2.loli.net/2022/12/07/rMpck9bReB4xd81.png)

![image-20221206165035986](https://s2.loli.net/2022/12/07/lQU2Frm4ZwvSfty.png)

![image-20221206165044151](https://s2.loli.net/2022/12/07/hwbcIXugW3zOTki.png)

![image-20221207144504296](https://s2.loli.net/2022/12/07/xvQC7HUe1hLTsft.png)

#### 后处理Outline

![202212191204652.png](https://s2.loli.net/2022/12/19/BDfkm4auvOV5bQU.png)

### 参考

[烟雨迷离半世殇的成长之路 - 图形专题部分内容](https://www.lfzxb.top/categories/%E5%9B%BE%E5%BD%A2%E6%B8%B2%E6%9F%93/)

[一骑gens - B站专栏](https://space.bilibili.com/5863867/article)

[毛星云 - XPostProcessingLibrary](https://github.com/QianMo/X-PostProcessing-Library)
