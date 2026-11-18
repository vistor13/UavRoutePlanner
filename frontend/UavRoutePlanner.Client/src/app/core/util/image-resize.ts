export const MAX_SOURCE_BYTES = 15 * 1024 * 1024;

const TARGET_SIDE = 480;

export async function toSquarePhoto(file: File): Promise<Blob> {
  const bitmap = await createImageBitmap(file);
  try {
    const side = Math.min(bitmap.width, bitmap.height);
    const target = Math.min(TARGET_SIDE, side);
    const canvas = document.createElement('canvas');
    canvas.width = target;
    canvas.height = target;

    const context = canvas.getContext('2d');
    if (!context) {
      throw new Error('Canvas is not available.');
    }

    context.imageSmoothingQuality = 'high';
    context.drawImage(
      bitmap,
      (bitmap.width - side) / 2,
      (bitmap.height - side) / 2,
      side,
      side,
      0,
      0,
      target,
      target
    );

    return await new Promise<Blob>((resolve, reject) =>
      canvas.toBlob(blob => (blob ? resolve(blob) : reject(new Error('Image export failed.'))), 'image/webp', 0.86)
    );
  } finally {
    bitmap.close();
  }
}
