export const RELOAD_FILTER_LIST = 'RELOAD_FILTER_LIST';
export type RELOAD_FILTER_LIST = typeof RELOAD_FILTER_LIST;

export interface ReloadFilterList {
    type: RELOAD_FILTER_LIST;
}

export type FilterAction = ReloadFilterList;

export function ReloadFilterList(): ReloadFilterList {
    return {
        type: RELOAD_FILTER_LIST
    }
}