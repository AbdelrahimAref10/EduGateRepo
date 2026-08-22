import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ImageService {
  readonly emptyAvatar = 'assets/images/avatar-empty.svg';
  readonly maxBytes = 5 * 1024 * 1024;
  private readonly allowed = new Set(['image/jpeg', 'image/png', 'image/webp']);

  display(value?: string | null): string {
    const photo = value?.trim();
    if (!photo) return this.emptyAvatar;
    if (photo.startsWith('data:image/')) return photo;
    return this.emptyAvatar;
  }

  fromPicker(event: Event): Promise<string> {
    const input = event.target as HTMLInputElement;
    const selected = input.files?.[0];
    input.value = '';

    if (!selected) return Promise.reject(new Error('EMPTY'));
    if (!this.allowed.has(selected.type) || selected.size > this.maxBytes) {
      return Promise.reject(new Error('INVALID'));
    }

    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = String(reader.result ?? '');
        if (!result.startsWith('data:image/')) {
          reject(new Error('INVALID'));
          return;
        }
        resolve(result);
      };
      reader.onerror = () => reject(new Error('INVALID'));
      reader.readAsDataURL(selected);
    });
  }
}
