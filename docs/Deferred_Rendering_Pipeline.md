frame start

for each camera:
- create/update size of g-buffer
- set RenderingCamera to this
- update graphics matrices
- clear RenderingCamera
- prepare the rendering pipeline

- for each object:
  - invoke OnPreRender
  - if light:
    - for all objects:
      - call OnRenderObjectDepth
        - render object to shadow map

- for each object:
  - if not light:
    - render to g-buffer

- for each object:
  - invoke OnPostRender

- render the render pipeline:
  - 