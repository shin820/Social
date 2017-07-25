import { FilterAction } from '../actions';
import { ApplicationState } from '../types/index';
import { RELOAD_FILTER_LIST } from '../constants/index';
import { FilterListItem } from '../types/index';

export function filter(state: ApplicationState, action: FilterAction): ApplicationState {
    switch (action.type) {
        case RELOAD_FILTER_LIST:
            return { FilterList: [new FilterListItem(1, "test", 10), new FilterListItem(2, "test2", 20)] };
    }
    return state;
}