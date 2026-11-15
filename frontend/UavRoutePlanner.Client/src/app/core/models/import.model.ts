export interface ImportedObject {
  name: string;
  latitude: number;
  longitude: number;
}

export interface ImportError {
  line: number;
  message: string;
}

export interface ImportResult {
  objects: ImportedObject[];
  errors: ImportError[];
}
