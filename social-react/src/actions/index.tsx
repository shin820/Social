import * as constants from '../constants'

export interface ReloadFilterList {
    type: constants.RELOAD_FILTER_LIST;
}

export type FilterAction = ReloadFilterList;

export function ReloadFilterList(): ReloadFilterList {
    return {
        type: constants.RELOAD_FILTER_LIST
    }
}