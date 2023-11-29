import { toast, type ToastOptions, type ToastType } from 'vue3-toastify';

const createDefaultToastOptions = (type: ToastType): ToastOptions => ({
  type,
  position: toast.POSITION.BOTTOM_CENTER,
  autoClose: 5000,
  transition: toast.TRANSITIONS.ZOOM,
  theme: toast.THEME.COLORED,
});

export const defaultToastOptions = {
  info: createDefaultToastOptions(toast.TYPE.INFO),
  success: createDefaultToastOptions(toast.TYPE.SUCCESS),
  warning: createDefaultToastOptions(toast.TYPE.WARNING),
  error: createDefaultToastOptions(toast.TYPE.ERROR),
};
