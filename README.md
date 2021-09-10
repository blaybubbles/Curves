# Curves interpolations library 

Unity Library contains spline and Bézier functions for creating a smooth 3d 
curves inspired by three.js

![Image](/Assets/BB/CurveInterpolations/img/img1.jpg)


## CurveInterpolations 

Static utility class providing methods for working with curves

- CatmullRom

  Returns a point on 3d spline curve using the Catmull-Rom algorithm. (Centripetal Catmull Rom - https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline)
  
- QuadraticBezier 

  Returns a point on quadratic bezier curve, defined by a startpoint, endpoint and a single control point. (https://en.wikipedia.org/wiki/B%C3%A9zier_curve)
  
- CubicBezier
  
  Returns a point on cubic bezier curve, defined by a start point, endpoint and two control points. (https://en.wikipedia.org/wiki/B%C3%A9zier_curve)
  
## Implemented classes
CubicBezierCurve - Contains a smooth 3d cubic bezier curve, defined by a start point, endpoint and two 
control points.

SplineCurve - Contains a smooth 3d spline curve from a series of points using the Catmull-Rom 
algorithm.

CubicBezierCurvePath - Contains a smooth 3d cubic bezier curves from a series of points. Control points calculate previos and next points
