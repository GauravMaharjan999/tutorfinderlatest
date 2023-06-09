/*
 * FilePondPluginImagePreview 2.0.1
 * Licensed under MIT, https://opensource.org/licenses/MIT
 * Please visit https://pqina.nl/filepond for details.
 */
// test if file is of type image and can be viewed in canvas
const isPreviewableImage = file => /^image/.test(file.type); // && !/svg/.test(file.type);

const transforms = {
  1: () => [1, 0, 0, 1, 0, 0],
  2: width => [-1, 0, 0, 1, width, 0],
  3: (width, height) => [-1, 0, 0, -1, width, height],
  4: (width, height) => [1, 0, 0, -1, 0, height],
  5: () => [0, 1, 1, 0, 0, 0],
  6: (width, height) => [0, 1, -1, 0, height, 0],
  7: (width, height) => [0, -1, -1, 0, height, width],
  8: width => [0, -1, 1, 0, 0, width]
};

const fixImageOrientation = (ctx, width, height, orientation) => {
  // no orientation supplied
  if (orientation === -1) {
    return;
  }

  ctx.transform(...transforms[orientation](width, height));
};

// draws the preview image
const createPreviewImage = (data, width, height, orientation) => {
  // round
  width = Math.round(width);
  height = Math.round(height);

  // width and height have already been swapped earlier
  // if orientation was in range below, let's swap back to make
  // this code a bit more readable
  if (orientation >= 5 && orientation <= 8) {
    [width, height] = [height, width];
  }

  // draw image
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');

  // if is rotated incorrectly swap width and height
  if (orientation >= 5 && orientation <= 8) {
    canvas.width = height;
    canvas.height = width;
  } else {
    canvas.width = width;
    canvas.height = height;
  }

  // correct image orientation
  ctx.save();
  fixImageOrientation(ctx, width, height, orientation);

  // draw the image
  ctx.drawImage(data, 0, 0, width, height);

  // end draw image
  ctx.restore();

  // data has been transferred to canvas ( if was ImageBitmap )
  if ('close' in data) {
    data.close();
  }

  return canvas;
};

const isBitmap = file => /^image/.test(file.type) && !/svg/.test(file.type);

const IMAGE_SCALE_SPRING_PROPS = {
  type: 'spring',
  stiffness: 0.5,
  damping: 0.45,
  mass: 10
};

const createImageView = fpAPI =>
  fpAPI.utils.createView({
    name: 'image-preview',
    tag: 'div',
    ignoreRect: true,
    create: ({ root, props }) => {
      root.ref.clip = document.createElement('div');
      root.element.appendChild(root.ref.clip);

      const transparencyIndicator = root.query(
        'GET_IMAGE_PREVIEW_TRANSPARENCY_INDICATOR'
      );
      if (transparencyIndicator === null) {
        return;
      }

      // grid pattern
      if (transparencyIndicator === 'grid') {
        root.element.dataset.transparencyIndicator = transparencyIndicator;
      } else {
        // basic color
        root.element.dataset.transparencyIndicator = 'color';
      }
    },
    write: fpAPI.utils.createRoute({
      DID_IMAGE_PREVIEW_LOAD: ({ root, props, action }) => {
        const { id } = props;

        // get item
        const item = root.query('GET_ITEM', { id: props.id });

        // should render background color
        const transparencyIndicator = root.query(
          'GET_IMAGE_PREVIEW_TRANSPARENCY_INDICATOR'
        );

        // orientation info
        const exif = item.getMetadata('exif') || {};
        const orientation = exif.orientation || -1;

        // get width and height from action, and swap of orientation is incorrect
        let { width, height } = action.data;
        if (orientation >= 5 && orientation <= 8) {
          [width, height] = [height, width];
        }

        // get item props
        const crop = item.getMetadata('crop') || {
          rect: {
            x: 0,
            y: 0,
            width: 1,
            height: 1
          },
          aspectRatio: height / width
        };

        // scale canvas based on pixel density
        const pixelDensityFactor = window.devicePixelRatio;

        // the max height of the preview container
        const fixedPreviewHeight = root.query('GET_IMAGE_PREVIEW_HEIGHT');
        const minPreviewHeight = root.query('GET_IMAGE_PREVIEW_MIN_HEIGHT');
        const maxPreviewHeight = root.query('GET_IMAGE_PREVIEW_MAX_HEIGHT');

        // calculate scaled preview image size
        const containerWidth = root.rect.inner.width;
        const previewImageRatio = height / width;
        const previewWidth = containerWidth;
        const previewHeight = containerWidth * previewImageRatio;

        // calculate image preview height and width
        const imageHeight =
          fixedPreviewHeight !== null
            ? fixedPreviewHeight
            : Math.max(minPreviewHeight, Math.min(height, maxPreviewHeight));
        const imageWidth = imageHeight / previewImageRatio;

        // render scaled preview image
        const previewImage = isBitmap(item.file)
          ? createPreviewImage(
              action.data,
              imageWidth * pixelDensityFactor,
              imageHeight * pixelDensityFactor,
              orientation
            )
          : action.data;

        // calculate crop container size
        let clipHeight =
          fixedPreviewHeight !== null
            ? fixedPreviewHeight
            : Math.max(
                minPreviewHeight,
                Math.min(containerWidth * crop.aspectRatio, maxPreviewHeight)
              );

        let clipWidth = clipHeight / crop.aspectRatio;
        if (clipWidth > previewWidth) {
          clipWidth = previewWidth;
          clipHeight = clipWidth * crop.aspectRatio;
        }

        // calculate scalar based on if the clip rectangle has been scaled down
        const previewScalar = clipHeight / (previewHeight * crop.rect.height);

        width = previewWidth * previewScalar;
        height = previewHeight * previewScalar;
        const x = -crop.rect.x * previewWidth * previewScalar;
        const y = -crop.rect.y * previewHeight * previewScalar;

        // apply styles
        root.ref.clip.style.cssText = `
                    width: ${Math.round(clipWidth)}px;
                    height: ${Math.round(clipHeight)}px;
                `;

        // position image
        previewImage.style.cssText = `
                    ${
                      transparencyIndicator !== null &&
                      transparencyIndicator !== 'grid'
                        ? 'background-color: ' + transparencyIndicator + ';'
                        : ''
                    }
                    width: ${Math.round(width)}px;
                    height: ${Math.round(height)}px;
                    transform: translate(${Math.round(x)}px, ${Math.round(
          y
        )}px) rotateZ(0.00001deg);
                `;
        root.ref.clip.appendChild(previewImage);

        // let others know of our fabulous achievement (so the image can be faded in)
        root.dispatch('DID_IMAGE_PREVIEW_DRAW', { id });
      }
    }),
    mixins: {
      styles: ['scaleX', 'scaleY', 'opacity'],
      animations: {
        scaleX: IMAGE_SCALE_SPRING_PROPS,
        scaleY: IMAGE_SCALE_SPRING_PROPS,
        opacity: { type: 'tween', duration: 750 }
      }
    }
  });

/**
 * Create gradient and mask definitions, we use these in each overlay so we can define them once
 * Turns out this also helps Safari to render the gradient on time
 */
const definitions = `<radialGradient id="filepond--image-preview-radial-gradient" cx=".5" cy="1.25" r="1.15">
<stop offset='50%' stop-color='#000000'/>
<stop offset='56%' stop-color='#0a0a0a'/>
<stop offset='63%' stop-color='#262626'/>
<stop offset='69%' stop-color='#4f4f4f'/>
<stop offset='75%' stop-color='#808080'/>
<stop offset='81%' stop-color='#b1b1b1'/>
<stop offset='88%' stop-color='#dadada'/>
<stop offset='94%' stop-color='#f6f6f6'/>
<stop offset='100%' stop-color='#ffffff'/>
</radialGradient>

<mask id="filepond--image-preview-masking">
<rect x="0" y="0" width="500" height="200" fill="url(#filepond--image-preview-radial-gradient)"></rect>
</mask>`;

const appendDefinitions = () => {
  if (document.readyState === 'interactive') {
    const defs = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    defs.style.cssText = 'position:absolute;width:0;height:0';
    defs.innerHTML = definitions;
    document.body.appendChild(defs);
  }
};

const hasNavigator = typeof navigator !== 'undefined';
if (hasNavigator) {
  appendDefinitions();
  document.addEventListener('readystatechange', appendDefinitions);
}

// need to know if this is IE11 so we can render the definitions with each overlay
const isEdgeOrIE = hasNavigator
  ? document.documentMode || /Edge/.test(navigator.userAgent)
  : false;

const createImageOverlayView = fpAPI =>
  fpAPI.utils.createView({
    name: 'image-preview-overlay',
    tag: 'div',
    ignoreRect: true,
    create: ({ root, props }) => {
      root.element.classList.add(
        `filepond--image-preview-overlay-${props.status}`
      );
      root.element.innerHTML = `<svg width="500" height="200" viewBox="0 0 500 200" preserveAspectRatio="none">
                ${isEdgeOrIE ? `<defs>${definitions}</defs>` : ''}
                <rect x="0" width="500" height="200" fill="currentColor" mask="url(#filepond--image-preview-masking)"></rect>
            </svg>
            `;
    },
    mixins: {
      styles: ['opacity'],
      animations: {
        opacity: { type: 'spring', mass: 25 }
      }
    }
  });

/**
 * Bitmap Worker
 */
const BitmapWorker = function() {
  // route messages
  self.onmessage = e => {
    toBitmap(e.data.message, response => {
      // imageBitmap is sent back as transferable
      self.postMessage({ id: e.data.id, message: response }, [response]);
    });
  };

  // resize image data
  const toBitmap = (options, cb) => {
    fetch(options.file)
      .then(response => response.blob())
      .then(blob => createImageBitmap(blob))
      .then(imageBitmap => cb(imageBitmap));
  };
};

const getImageSize = (url, cb) => {
  let image = new Image();
  image.onload = () => {
    const width = image.naturalWidth;
    const height = image.naturalHeight;
    image = null;
    cb(width, height);
  };
  image.src = url;
};

const canCreateImageBitmap = file =>
  'createImageBitmap' in window && isBitmap(file);

const createImageWrapperView = _ => {
  // create overlay view
  const overlay = createImageOverlayView(_);

  /**
   * Write handler for when preview container has been created
   */
  const didCreatePreviewContainer = ({ root, props }) => {
    const { utils } = _;
    const { createWorker, loadImage } = utils;
    const { id } = props;

    // we need to get the file data to determine the eventual image size
    const item = root.query('GET_ITEM', id);

    // get url to file (we'll revoke it later on when done)
    const fileURL = URL.createObjectURL(item.file);

    // fallback
    const loadPreviewFallback = (item, width, height, orientation) => {
      // let's scale the image in the main thread :(
      loadImage(fileURL).then(previewImageLoaded);
    };

    // image is now ready
    const previewImageLoaded = data => {
      // the file url is no longer needed
      URL.revokeObjectURL(fileURL);

      // the preview is now ready to be drawn
      root.dispatch('DID_IMAGE_PREVIEW_LOAD', {
        id,
        data
      });
    };

    // determine image size of this item
    getImageSize(fileURL, (width, height) => {
      // we can now scale the panel to the final size
      root.dispatch('DID_IMAGE_PREVIEW_CALCULATE_SIZE', {
        id,
        width,
        height
      });

      // if we support scaling using createImageBitmap we use a worker
      if (canCreateImageBitmap(item.file)) {
        // let's scale the image in a worker
        const worker = createWorker(BitmapWorker);
        worker.post(
          {
            file: fileURL
          },
          imageBitmap => {
            // destroy worker
            worker.terminate();

            // no bitmap returned, must be something wrong,
            // try the oldschool way
            if (!imageBitmap) {
              loadPreviewFallback(item);
              return;
            }

            // yay we got our bitmap, let's continue showing the preview
            previewImageLoaded(imageBitmap);
          }
        );
      } else {
        // create fallback preview
        loadPreviewFallback(item);
      }
    });
  };

  /**
   * Write handler for when the preview has been loaded
   */
  const didLoadPreview = ({ root }) => {
    root.ref.overlayShadow.opacity = 1;
  };

  /**
   * Write handler for when the preview image is ready to be animated
   */
  const didDrawPreview = ({ root }) => {
    const { image } = root.ref;

    // reveal image
    image.scaleX = 1.0;
    image.scaleY = 1.0;
    image.opacity = 1;
  };

  /**
   * Write handler for when the preview has been loaded
   */
  const restoreOverlay = ({ root }) => {
    root.ref.overlayShadow.opacity = 1;
    root.ref.overlayError.opacity = 0;
    root.ref.overlaySuccess.opacity = 0;
  };

  const didThrowError = ({ root }) => {
    root.ref.overlayShadow.opacity = 0.25;
    root.ref.overlayError.opacity = 1;
  };

  const didCompleteProcessing = ({ root }) => {
    root.ref.overlayShadow.opacity = 0.25;
    root.ref.overlaySuccess.opacity = 1;
  };

  /**
   * Constructor
   */
  const create = ({ root, props }) => {
    // image view
    const image = createImageView(_);

    // append image presenter
    root.ref.image = root.appendChildView(
      root.createChildView(image, {
        id: props.id,
        scaleX: 1.25,
        scaleY: 1.25,
        opacity: 0
      })
    );

    // image overlays
    root.ref.overlayShadow = root.appendChildView(
      root.createChildView(overlay, {
        opacity: 0,
        status: 'idle'
      })
    );

    root.ref.overlaySuccess = root.appendChildView(
      root.createChildView(overlay, {
        opacity: 0,
        status: 'success'
      })
    );

    root.ref.overlayError = root.appendChildView(
      root.createChildView(overlay, {
        opacity: 0,
        status: 'failure'
      })
    );
  };

  return _.utils.createView({
    name: 'image-preview-wrapper',
    create,
    write: _.utils.createRoute({
      // image preview stated
      DID_IMAGE_PREVIEW_LOAD: didLoadPreview,
      DID_IMAGE_PREVIEW_DRAW: didDrawPreview,
      DID_IMAGE_PREVIEW_CONTAINER_CREATE: didCreatePreviewContainer,

      // file states
      DID_THROW_ITEM_LOAD_ERROR: didThrowError,
      DID_THROW_ITEM_PROCESSING_ERROR: didThrowError,
      DID_THROW_ITEM_INVALID: didThrowError,
      DID_COMPLETE_ITEM_PROCESSING: didCompleteProcessing,
      DID_START_ITEM_PROCESSING: restoreOverlay,
      DID_REVERT_ITEM_PROCESSING: restoreOverlay
    })
  });
};

/**
 * Image Preview Plugin
 */
var plugin$1 = fpAPI => {
  const { addFilter, utils } = fpAPI;
  const { Type, createRoute, isFile } = utils;

  // imagePreviewView
  const imagePreviewView = createImageWrapperView(fpAPI);

  // called for each view that is created right after the 'create' method
  addFilter('CREATE_VIEW', viewAPI => {
    // get reference to created view
    const { is, view, query } = viewAPI;

    // only hook up to item view and only if is enabled for this cropper
    if (!is('file') || !query('GET_ALLOW_IMAGE_PREVIEW')) {
      return;
    }

    // create the image preview plugin, but only do so if the item is an image
    const didLoadItem = ({ root, props, actions }) => {
      const { id } = props;
      const item = query('GET_ITEM', id);

      // item could theoretically have been removed in the mean time
      if (!item || !isFile(item.file)) {
        return;
      }

      // get the file object
      const file = item.file;

      // exit if this is not an image
      if (!isPreviewableImage(file)) {
        return;
      }

      // exit if image size is too high and no createImageBitmap support
      // this would simply bring the browser to its knees and that is not what we want
      const supportsCreateImageBitmap = 'createImageBitmap' in (window || {});
      const maxPreviewFileSize = query('GET_IMAGE_PREVIEW_MAX_FILE_SIZE');
      if (
        !supportsCreateImageBitmap &&
        maxPreviewFileSize &&
        file.size > maxPreviewFileSize
      ) {
        return;
      }

      // set preview view
      root.ref.imagePreview = view.appendChildView(
        view.createChildView(imagePreviewView, { id })
      );

      // now ready
      root.dispatch('DID_IMAGE_PREVIEW_CONTAINER_CREATE', { id });
    };

    const didCalculatePreviewSize = ({ root, props, action }) => {
      // get item
      const item = root.query('GET_ITEM', { id: props.id });

      // orientation info
      const exif = item.getMetadata('exif') || {};
      const orientation = exif.orientation || -1;

      // get width and height from action, and swap of orientation is incorrect
      let { width, height } = action;
      if (orientation >= 5 && orientation <= 8) {
        [width, height] = [height, width];
      }

      // we need the item to get to the crop size
      const crop = item.getMetadata('crop') || {
        rect: {
          x: 0,
          y: 0,
          width: 1,
          height: 1
        },
        aspectRatio: height / width
      };

      // get height min and max
      const fixedPreviewHeight = root.query('GET_IMAGE_PREVIEW_HEIGHT');
      const minPreviewHeight = root.query('GET_IMAGE_PREVIEW_MIN_HEIGHT');
      const maxPreviewHeight = root.query('GET_IMAGE_PREVIEW_MAX_HEIGHT');

      // scale up width and height when we're dealing with an SVG
      if (!isBitmap(item.file)) {
        const scalar = 2048 / width;
        width *= scalar;
        height *= scalar;
      }

      // const crop width
      height =
        fixedPreviewHeight !== null
          ? fixedPreviewHeight
          : Math.max(minPreviewHeight, Math.min(height, maxPreviewHeight));

      width = height / crop.aspectRatio;
      if (width > root.rect.element.width) {
        width = root.rect.element.width;
        height = width * crop.aspectRatio;
      }

      // set height
      root.ref.imagePreview.element.style.cssText = `height:${Math.round(
        height
      )}px`;
    };

    // start writing
    view.registerWriter(
      createRoute({
        DID_LOAD_ITEM: didLoadItem,
        DID_IMAGE_PREVIEW_CALCULATE_SIZE: didCalculatePreviewSize
      })
    );
  });

  // expose plugin
  return {
    options: {
      // Enable or disable image preview
      allowImagePreview: [true, Type.BOOLEAN],

      // Fixed preview height
      imagePreviewHeight: [null, Type.INT],

      // Min image height
      imagePreviewMinHeight: [44, Type.INT],

      // Max image height
      imagePreviewMaxHeight: [256, Type.INT],

      // Max size of preview file for when createImageBitmap is not supported
      imagePreviewMaxFileSize: [null, Type.INT],

      // Style of the transparancy indicator used behind images
      imagePreviewTransparencyIndicator: [null, Type.STRING]
    }
  };
};

if (typeof navigator !== 'undefined' && document) {
  // plugin has loaded
  document.dispatchEvent(
    new CustomEvent('FilePond:pluginloaded', { detail: plugin$1 })
  );
}

export default plugin$1;
